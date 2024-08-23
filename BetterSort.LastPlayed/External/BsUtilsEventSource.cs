using BetterSort.Common.External;
using BetterSort.Common.Models;
using BS_Utils.Utilities;
using SiraUtil.Logging;
using System;
using Zenject;

namespace BetterSort.LastPlayed.External {

  public interface IPlayEventSource {

    event Action<LastPlayRecord> OnSongPlayed;
  }

  internal class BsUtilsEventSource(IClock clock, SiraLog logger, Scoresaber scoresaber, Beatleader beatleader) : IPlayEventSource, IInitializable, IDisposable {
    private DateTime _startTime;

    public event Action<LastPlayRecord> OnSongPlayed = delegate { };

    public void Initialize() {
      BSEvents.gameSceneLoaded += RecordStartTime;
      BSEvents.LevelFinished += DispatchIfLongEnough;
    }

    public void Dispose() {
      BSEvents.LevelFinished -= DispatchIfLongEnough;
      BSEvents.gameSceneLoaded -= RecordStartTime;
    }

    private void RecordStartTime() {
      _startTime = clock.Now;
    }

    private void DispatchIfLongEnough(object sender, LevelFinishedEventArgs finished) {
      if (finished is not LevelFinishedWithResultsEventArgs) {
        logger.Info($"Skip tutorial play record.");
        return;
      }
      if (scoresaber.IsInReplay()) {
        logger.Info($"Skip scoresaber replay record.");
        return;
      }
      if (beatleader.IsInReplay()) {
        logger.Info($"Skip beatleader replay record.");
        return;
      }

      var setup = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
      if (setup == null) {
        logger.Warn($"Skip because cannot query game stats.");
        return;
      }

      var now = clock.Now;
      var duration = now - _startTime;
      var preview = setup.previewBeatmapLevel;
      string songName = preview.songName;
      float songDuration = preview.songDuration;
      bool isPlayedTooShort = duration.TotalSeconds < 10 && songDuration > 10;
      if (isPlayedTooShort) {
        logger.Info($"Skip record due to too short play: {songName}");
        return;
      }

      logger.Info($"Dispatch play event: {songName}");

      var diffBeatmap = setup.difficultyBeatmap;
      string levelId = preview.levelID;
      string type = diffBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
      var difficulty = RecordDifficultyExtension.ConvertFromString(diffBeatmap.difficulty.SerializedName()) ?? RecordDifficulty.ExpertPlus;
      var lastPlay = new LastPlayRecord(now, levelId, new PlayedMap(type, difficulty));
      OnSongPlayed(lastPlay);
    }
  }
}
