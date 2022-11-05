using BetterSort.Accuracy.Sorter;
using BS_Utils.Utilities;
using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.Linq;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.External {
  using BetterSort.Common.External;
  using UnityEngine;

  public record class PlayRecord(string LevelId, string Mode, RecordDifficulty Difficulty, double Accuracy);

  public interface IBsInterop : IDisposable {
    event Action<PlayRecord> OnSongPlayed;
    event Action<int, IPreviewBeatmapLevel> OnSongSelected;

    Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty);
  }

  internal class BsUtilsInterop : IBsInterop {
    public event Action<PlayRecord> OnSongPlayed = delegate { };
    public event Action<int, IPreviewBeatmapLevel> OnSongSelected = delegate { };

    public BsUtilsInterop(IPALogger logger, Scoresaber scoresaber, Beatleader beatleader) {
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      BSEvents.levelCleared += DispatchWithAccuracy;
      BSEvents.levelSelected += DispatchLevelSelection;
      BSEvents.characteristicSelected += DispatchCharacteristicSelection;
    }

    private void DispatchCharacteristicSelection(BeatmapCharacteristicSegmentedControlController arg1, BeatmapCharacteristicSO arg2) {
    }

    public void Dispose() {
      BSEvents.levelSelected -= DispatchLevelSelection;
      BSEvents.levelCleared -= DispatchWithAccuracy;
    }

    private readonly IPALogger _logger;
    private readonly Scoresaber _scoresaber;
    private readonly Beatleader _beatleader;
    public async Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty) {
      await UnityMainThreadTaskScheduler.Factory.StartNew(() => {
        var view = Object.FindObjectOfType<StandardLevelDetailView>();
        _logger.Debug($"{nameof(SetModeAndDifficulty)}: view first {view}");
        view = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
        _logger.Debug($"{nameof(SetModeAndDifficulty)}: view second {view}");
        var player = Object.FindObjectOfType<PlayerDataModel>();
        var level = view?.GetField<IBeatmapLevel, StandardLevelDetailView>("_level");
        view?.SetContent(level, difficulty.ToGameDifficulty() ?? global::BeatmapDifficulty.ExpertPlus, mode, player.playerData);
      });
    }

    private void DispatchLevelSelection(LevelCollectionViewController controller, IPreviewBeatmapLevel level) {
      try {
        // highlight: BeatmapDifficultySegmentedControlController
        var view = controller.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
        int index = view.GetField<int, LevelCollectionTableView>("_selectedRow");
        _logger.Info($"SongSelected: {index} {level.songName}");
        OnSongSelected(index, level);
      }
      catch (Exception ex) {
        _logger.Error(ex);
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
