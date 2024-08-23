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

    private static LastPlayRecord MakeRecord(IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel preview, DateTime now) {
      string levelId = preview.levelID;
      string type = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
      var difficulty = RecordDifficultyExtension.ConvertFromString(difficultyBeatmap.difficulty.SerializedName()) ?? RecordDifficulty.ExpertPlus;
      return new LastPlayRecord(now, levelId, new PlayedMap(type, difficulty));
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

      var preview = setup.previewBeatmapLevel;
      var now = clock.Now;
      string? reason = GetSkipReason(finished, preview, now);
      if (reason != null) {
        logger.Info(reason);
        return;
      }

      logger.Info($"Dispatch play event: {preview.songName}");
      OnSongPlayed(MakeRecord(setup.difficultyBeatmap, preview, now));
    }

    private string? GetSkipReason(LevelFinishedEventArgs finished, IPreviewBeatmapLevel preview, DateTime now) {
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
      float songDuration = preview.songDuration;
      bool isPlayedTooShort = duration.TotalSeconds < 10 && songDuration > 10;
      if (isPlayedTooShort) {
        return $"Skip record due to too short play: {preview.songName}";
      }

      return null;
    }
  }
}
