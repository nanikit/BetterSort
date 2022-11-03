using BS_Utils.Utilities;
using System;
using IPALogger = IPA.Logging.Logger;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {
  using BetterSort.Common.External;

  public record class PlayRecord(string LevelId, string Mode, BeatmapDifficulty Difficulty, double Accuracy);

  public interface IPlayEventSource : IDisposable {
    event Action<PlayRecord> OnSongPlayed;
  }

  internal class BsUtilsEventSource : IPlayEventSource {
    public event Action<PlayRecord> OnSongPlayed = delegate { };

    public BsUtilsEventSource(IPALogger logger, Scoresaber scoresaber, Beatleader beatleader) {
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      //BSEvents.characteristicSelected += PreserveMode;
      //BSEvents.difficultySelected += PreserveDifficulty;
      BSEvents.levelCleared += DispatchWithAccuracy;
    }

    public void Dispose() {
      BSEvents.levelCleared -= DispatchWithAccuracy;
      //BSEvents.difficultySelected -= PreserveDifficulty;
      //BSEvents.characteristicSelected -= PreserveMode;
    }

    private readonly IPALogger _logger;
    private readonly Scoresaber _scoresaber;
    private readonly Beatleader _beatleader;
    private string _selectedMode = "";
    private BeatmapDifficulty _selectedDifficulty = BeatmapDifficulty.Easy;

    //private void PreserveMode(BeatmapCharacteristicSegmentedControlController _, BeatmapCharacteristicSO mode) {
    //  _selectedMode = mode.serializedName;
    //}

    //private void PreserveDifficulty(StandardLevelDetailViewController arg1, IDifficultyBeatmap difficulty) {
    //  string name = difficulty.difficulty.SerializedName();
    //  var diff = DifficultyExtension.ConvertFromString(name);
    //  if (diff is BeatmapDifficulty diffi) {
    //    _selectedDifficulty = diffi;
    //  }
    //  else {
    //    _logger.Warn($"Unknown difficulty {name}, regard it as E+.");
    //    _selectedDifficulty = BeatmapDifficulty.ExpertPlus;
    //  }
    //}

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
      var difficulty = setup.difficultyBeatmap;
      if (difficulty == null) {
        _logger.Warn($"Skip record because cannot query difficulty.");
        return;
      }

      string? levelId = difficulty.level?.levelID;
      if (levelId == null) {
        _logger.Warn($"Cannot determine selected level");
        return;
      }
      string? songName = difficulty.level?.songName;

      string? mode = difficulty.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName;
      if (mode == null) {
        _logger.Warn($"Cannot determine selected mode: {levelId} {songName}");
        return;
      }
      var diffi = DifficultyExtension.ConvertFromString(difficulty.difficulty.Name());
      if (diffi is not BeatmapDifficulty selectedDiff) {
        _logger.Warn($"Cannot determine selected difficulty: {levelId} {songName} {mode}");
        return;
      }

      var beatmap = await difficulty.GetBeatmapDataAsync(setup.environmentInfo, setup.playerSpecificSettings).ConfigureAwait(false);
      if (beatmap == null) {
        _logger.Warn($"Skip record because cannot query beatmap: {levelId} {songName}");
        return;
      }
      int maxMultiplied = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmap);
      double accuracy = result.multipliedScore / maxMultiplied;

      _logger.Debug($"Dispatch play event: {songName ?? "(null)"} {_selectedMode} {_selectedDifficulty} {accuracy}");
      OnSongPlayed(new PlayRecord(levelId, mode, selectedDiff, accuracy));
    }
  }
}
