using BetterSort.Common.External;
using BetterSort.Common.Models;
using BetterSort.LastPlayed.Sorter;
using BS_Utils.Utilities;
using SiraUtil.Logging;
using System;

namespace BetterSort.LastPlayed.External {

  public interface IPlayEventSource : IDisposable {

    event Action<LastPlayRecord> OnSongPlayed;
  }

  internal class BsUtilsEventSource : IPlayEventSource {
    private readonly IClock _clock;
    private readonly SiraLog _logger;
    private readonly Scoresaber _scoresaber;
    private readonly Beatleader _beatleader;

    private DateTime _startTime;

    public BsUtilsEventSource(IClock clock, SiraLog logger, Scoresaber scoresaber, Beatleader beatleader) {
      _clock = clock;
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      BSEvents.gameSceneLoaded += RecordStartTime;
      BSEvents.LevelFinished += DispatchIfLongEnough;
    }

    public event Action<LastPlayRecord> OnSongPlayed = delegate { };

    public void Dispose() {
      BSEvents.LevelFinished -= DispatchIfLongEnough;
      BSEvents.gameSceneLoaded -= RecordStartTime;
    }

    private void RecordStartTime() {
      _startTime = _clock.Now;
    }

    private void DispatchIfLongEnough(object sender, LevelFinishedEventArgs finished) {
      if (finished is not LevelFinishedWithResultsEventArgs) {
        _logger.Info($"Skip tutorial play record.");
        return;
      }
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
        _logger.Warn($"Skip because cannot query game stats.");
        return;
      }

      var now = _clock.Now;
      var duration = now - _startTime;
      var preview = setup.previewBeatmapLevel;
      string songName = preview.songName;
      float songDuration = preview.songDuration;
      bool isPlayedTooShort = duration.TotalSeconds < 10 && songDuration > 10;
      if (isPlayedTooShort) {
        _logger.Info($"Skip record due to too short play: {songName}");
        return;
      }

      _logger.Info($"Dispatch play event: {songName}");

      var diffBeatmap = setup.difficultyBeatmap;
      string levelId = preview.levelID;
      string type = diffBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
      var difficulty = RecordDifficultyExtension.ConvertFromString(diffBeatmap.difficulty.SerializedName()) ?? RecordDifficulty.ExpertPlus;
      var lastPlay = new LastPlayRecord(now, levelId, new PlayedMap(type, difficulty));
      OnSongPlayed(lastPlay);
    }
  }
}
