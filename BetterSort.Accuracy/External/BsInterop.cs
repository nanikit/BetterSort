using System;

namespace BetterSort.Accuracy.External {

  using BetterSort.Accuracy.Sorter;
  using BetterSort.Common.External;
  using BS_Utils.Gameplay;
  using BS_Utils.Utilities;
  using HarmonyLib;
  using IPA.Utilities;
  using IPA.Utilities.Async;
  using SiraUtil.Logging;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using UnityEngine;
  using Zenject;

  public record class PlayRecord(string LevelId, string Mode, RecordDifficulty Difficulty, double Accuracy);

  public delegate void OnSongSelectedHandler(int index, IPreviewBeatmapLevel level);

  public interface IBsInterop : IDisposable {

    event Action<PlayRecord> OnSongPlayed;

    event OnSongSelectedHandler OnSongSelected;

    Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty);

    void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels);
  }

  internal class BsUtilsInterop : IBsInterop {
    private static readonly MethodInfo _handleLevelCollectionViewControllerDidSelectLevel = AccessTools.Method(typeof(LevelCollectionNavigationController), "HandleLevelCollectionViewControllerDidSelectLevel");
    private static OnSongSelectedHandler? _onSongSelected;
    private readonly SiraLog _logger;

    private readonly Scoresaber _scoresaber;

    private readonly Beatleader _beatleader;

    private readonly Harmony _harmony;

    private bool _hasPlaylistManager = true;

    public BsUtilsInterop(SiraLog logger, Scoresaber scoresaber, Beatleader beatleader, [Inject(Id = "BetterSort.Accuracy.Harmony")] Harmony harmony) {
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      _harmony = harmony;
      BSEvents.levelCleared += DispatchWithAccuracy;
      BSEvents.characteristicSelected += DispatchCharacteristicSelection;
    }

    public event Action<PlayRecord> OnSongPlayed = delegate { };

    public event OnSongSelectedHandler OnSongSelected {
      add {
        if (_onSongSelected == null) {
          _logger.Debug($"{nameof(OnSongSelected)}: add listener with initializing hook.");
          _harmony.Patch(
            original: _handleLevelCollectionViewControllerDidSelectLevel,
            prefix: new HarmonyMethod(AccessTools.Method(
              typeof(BsUtilsInterop),
              nameof(HandleLevelCollectionViewControllerDidSelectLevelPrefix)
            ))
          );
        }
        else {
          _logger.Debug($"{nameof(OnSongSelected)}: add listener.");
        }

        _onSongSelected += value;
      }
      remove {
        _onSongSelected -= value;

        if (_onSongSelected == null) {
          _logger.Debug($"{nameof(OnSongSelected)}: remove listener and hook.");
          _harmony.Unpatch(_handleLevelCollectionViewControllerDidSelectLevel, HarmonyPatchType.Prefix, _harmony.Id);
        }
        else {
          _logger.Debug($"{nameof(OnSongSelected)} remove listener.");
        }
      }
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
        if (!_hasPlaylistManager) {
          return;
        }
        var type = Type.GetType("PlaylistManager.HarmonyPatches.LevelCollectionViewController_SetData, PlaylistManager");
        if (type == null) {
          _logger.Info($"{nameof(SetPlaylistItem)}: type is null, maybe there is no PlaylistManager mod.");
          _hasPlaylistManager = false;
          return;
        }
        string name = "beatmapLevels";
        var field = AccessTools.DeclaredField(type, name);
        if (field == null) {
          _logger.Warn($"{nameof(SetPlaylistItem)}: field {name} is not found");
          _hasPlaylistManager = false;
          return;
        }
        _logger.Debug($"{nameof(SetPlaylistItem)}: Try updating PlaylistManager levels.");
        AccessTools.StaticFieldRefAccess<IReadOnlyCollection<IPreviewBeatmapLevel>>(type, name) = levels;
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    public void Dispose() {
      BSEvents.levelCleared -= DispatchWithAccuracy;
    }

    private static void HandleLevelCollectionViewControllerDidSelectLevelPrefix(LevelCollectionViewController viewController, IPreviewBeatmapLevel level) {
      var view = viewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
      int row = view.GetField<int, LevelCollectionTableView>("_selectedRow");
      bool hasHeader = view.GetField<bool, LevelCollectionTableView>("_showLevelPackHeader");
      int index = hasHeader ? row - 1 : row;
      _onSongSelected?.Invoke(index, level);
    }

    private void DispatchCharacteristicSelection(BeatmapCharacteristicSegmentedControlController arg1, BeatmapCharacteristicSO arg2) {
    }

    private void DispatchWithAccuracy(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults result) {
      try {
        DispatchAccuracy(result);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private void DispatchAccuracy(LevelCompletionResults result) {
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
        _logger.Warn($"Skip record as querying game stats is not possible.");
        return;
      }

      var diffBeatmap = setup.difficultyBeatmap;
      var level = diffBeatmap?.level;
      string? levelId = level?.levelID;
      string? songName = level?.songName;
      string? mode = diffBeatmap?.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName;
      var difficulty = DifficultyExtension.ConvertFromString(diffBeatmap?.difficulty.SerializedName());
      if (levelId == null || difficulty is not RecordDifficulty diff || mode == null) {
        _logger.Warn($"Skip record as some game stats are missing: {levelId}, {mode}, {difficulty}, {songName}");
        return;
      }

      string levelDescription = $"{levelId} {songName}";
      if (setup.practiceSettings != null) {
        _logger.Info($"Skip practice record: {levelDescription}.");
        return;
      }

      if (ScoreSubmission.Disabled) {
        _logger.Info($"Skip record due to score submission being disabled by {ScoreSubmission.LastDisabledModString}: {levelDescription}.");
        return;
      }

      var transformed = setup.transformedBeatmapData;
      if (transformed == null) {
        _logger.Warn($"Skip record as accuracy is not available: {levelDescription}");
        return;
      }

      int maxMultiplied = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformed);
      double accuracy = (double)result.multipliedScore / maxMultiplied;

      _logger.Debug($"Dispatch play event: {songName ?? "(null)"} {mode} {diff} {accuracy}");
      OnSongPlayed?.Invoke(new PlayRecord(levelId, mode, diff, accuracy));
    }
  }
}
