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

    private static LastPlayRecord MakeRecord(string characteristic, string difficulty, string levelId, DateTime now) {
      var recordDifficulty = RecordDifficultyExtension.ConvertFromString(difficulty) ?? RecordDifficulty.ExpertPlus;
      return new LastPlayRecord(now, levelId, new PlayedMap(characteristic, recordDifficulty));
    }

    private void RecordStartTime() {
      _startTime = clock.Now;
    }

    private void DispatchIfLongEnough(object sender, LevelFinishedEventArgs finished) {
      var setup = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
      if (setup == null) {
        logger.Warn($"Skip because cannot query game stats.");
        return;
      }

#if NOT_BEFORE_1_36_2
      var preview = setup.beatmapLevel;
      string characteristic = setup.beatmapKey.beatmapCharacteristic.serializedName;
      string difficulty = setup.beatmapKey.difficulty.SerializedName();
#else
      var preview = setup.previewBeatmapLevel;
      string characteristic = setup.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
      string difficulty = setup.difficultyBeatmap.difficulty.SerializedName();
#endif

      var now = clock.Now;
      string? reason = GetSkipReason(finished, preview.songDuration, preview.songName, now);
      if (reason != null) {
        logger.Info(reason);
        return;
      }

      logger.Info($"Dispatch play event: {preview.songName}");
      OnSongPlayed(MakeRecord(characteristic, difficulty, preview.levelID, now));
    }

    private string? GetSkipReason(LevelFinishedEventArgs finished, float songDuration, string songName, DateTime now) {
      if (finished is not LevelFinishedWithResultsEventArgs) {
        return "Skip tutorial play record.";
      }
      if (scoresaber.IsInReplay()) {
        return "Skip scoresaber replay record.";
      }
      if (beatleader.IsInReplay()) {
        return "Skip beatleader replay record.";
      }

      var duration = now - _startTime;
      bool isPlayedTooShort = duration.TotalSeconds < 10 && songDuration > 10;
      if (isPlayedTooShort) {
        return $"Skip record due to too short play: {songName}";
      }

      return null;
    }
  }
}
