using System;

namespace BetterSort.Accuracy.External {
  using BetterSort.Accuracy.Sorter;
  using BetterSort.Common.External;
  using BS_Utils.Utilities;
  using HarmonyLib;
  using IPA.Utilities;
  using IPA.Utilities.Async;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using UnityEngine;
  using IPALogger = IPA.Logging.Logger;

  public record class PlayRecord(string LevelId, string Mode, RecordDifficulty Difficulty, double Accuracy);

  public interface IBsInterop : IDisposable {
    event Action<PlayRecord> OnSongPlayed;
    event Action<int, IPreviewBeatmapLevel> OnSongSelected;

    Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty);
    void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels);
  }

  internal class BsUtilsInterop : IBsInterop {
    private static readonly List<Action<int, IPreviewBeatmapLevel>> _onSongSelecteds = new();

    public event Action<PlayRecord> OnSongPlayed = delegate { };

    public event Action<int, IPreviewBeatmapLevel> OnSongSelected {
      add {
        if (_onSongSelecteds.Count == 0) {
          _harmony.Patch(
            original: AccessTools.Method(
              typeof(LevelCollectionNavigationController),
              nameof(LevelCollectionNavigationController.HandleLevelCollectionViewControllerDidSelectLevel)
            ),
            prefix: new HarmonyMethod(AccessTools.Method(
              typeof(BsUtilsInterop),
              nameof(HandleLevelCollectionViewControllerDidSelectLevelPrefix)
            ))
          );
        }
        _onSongSelecteds.Add(value);
      }
      remove {
        _onSongSelecteds.Remove(value);
        if (_onSongSelecteds.Count == 0) {
          _harmony.Unpatch(AccessTools.Method(
            typeof(LevelCollectionNavigationController),
            nameof(LevelCollectionNavigationController.HandleLevelCollectionViewControllerDidSelectLevel)
          ), HarmonyPatchType.Prefix);
        }
      }
    }

    public BsUtilsInterop(IPALogger logger, Scoresaber scoresaber, Beatleader beatleader, Harmony harmony) {
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      _harmony = harmony;
      BSEvents.levelCleared += DispatchWithAccuracy;
      BSEvents.characteristicSelected += DispatchCharacteristicSelection;
    }

    public async Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty) {
      await UnityMainThreadTaskScheduler.Factory.StartNew(() => {
        var player = Object.FindObjectOfType<PlayerDataModel>()?.playerData;
        if (player == null) {
          _logger.Warn("playerData is null. Quit.");
          return;
        }

        player.SetProperty(nameof(PlayerData.lastSelectedBeatmapCharacteristic), mode);
        player.SetProperty(nameof(PlayerData.lastSelectedBeatmapDifficulty), difficulty.ToGameDifficulty() ?? BeatmapDifficulty.ExpertPlus);
        var view = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
        var level = view?.GetField<IBeatmapLevel, StandardLevelDetailView>("_level");
        view?.SetContent(level, difficulty.ToGameDifficulty() ?? BeatmapDifficulty.ExpertPlus, mode, player);
      }).ConfigureAwait(false);
    }

    public void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels) {
      try {
        var type = Type.GetType("PlaylistManager.HarmonyPatches.LevelCollectionViewController_SetData, PlaylistManager");
        if (type == null) {
          _logger.Warn($"{nameof(SetPlaylistItem)}: type is null");
          return;
        }
        string name = "beatmapLevels";
        if (!AccessTools.GetFieldNames(type).Contains(name)) {
          _logger.Warn($"{nameof(SetPlaylistItem)}: field is not found");
          return;
        }
        AccessTools.StaticFieldRefAccess<IReadOnlyCollection<IPreviewBeatmapLevel>>(type, name) = levels;
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    public void Dispose() {
      BSEvents.levelCleared -= DispatchWithAccuracy;
    }

    private readonly IPALogger _logger;
    private readonly Scoresaber _scoresaber;
    private readonly Beatleader _beatleader;
    private readonly Harmony _harmony;

    private void DispatchCharacteristicSelection(BeatmapCharacteristicSegmentedControlController arg1, BeatmapCharacteristicSO arg2) {
    }

    private static void HandleLevelCollectionViewControllerDidSelectLevelPrefix(LevelCollectionViewController viewController, IPreviewBeatmapLevel level) {
      var view = viewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
      int index = view.GetField<int, LevelCollectionTableView>("_selectedRow") - 1;
      foreach (var action in _onSongSelecteds) {
        action(index, level);
      }
    }

    private async void DispatchWithAccuracy(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults result) {
      try {
        await DispatchAccuracy(result).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task DispatchAccuracy(LevelCompletionResults result) {
      if (_scoresaber.IsInReplay()) {
        _logger.Info($"Skip scoresaber replay record.");
        return;
      }
      if (_beatleader.IsInReplay()) {
        _logger.Info($"Skip beatleader replay record.");
        return;
      }

      var setup = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
      if (setup == null) {
        _logger.Warn($"Skip record because cannot query game stats.");
        return;
      }

      var diffBeatmap = setup.difficultyBeatmap;
      var level = diffBeatmap?.level;
      string? levelId = level?.levelID;
      string? songName = level?.songName;
      string? mode = diffBeatmap?.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName;
      var difficulty = DifficultyExtension.ConvertFromString(diffBeatmap?.difficulty.SerializedName());
      if (levelId == null || difficulty is not RecordDifficulty diff || mode == null) {
        _logger.Warn($"Skip record because cannot get info: {levelId}, {mode}, {difficulty}, {songName}");
        return;
      }

      var transformed = setup.transformedBeatmapData;
      transformed ??= await (setup.GetTransformedBeatmapDataAsync() ?? Task.FromResult<IReadonlyBeatmapData?>(null)).ConfigureAwait(false);
      if (transformed == null) {
        _logger.Warn($"Skip record because cannot query beatmap: {levelId} {songName}");
        return;
      }

      int maxMultiplied = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformed);
      double accuracy = (double)result.multipliedScore / maxMultiplied;

      _logger.Debug($"Dispatch play event: {songName ?? "(null)"} {mode} {diff} {accuracy}");
      OnSongPlayed(new PlayRecord(levelId, mode, diff, accuracy));
    }
  }
}
