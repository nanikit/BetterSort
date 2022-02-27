namespace BetterSongList.LastPlayedSort.External {
  using BS_Utils.Utilities;
  using System;

  public interface IPlayEventSource : IDisposable {
    event Action<string, DateTime> OnSongPlayed;
  }

  internal class BsUtilsEventSource : IPlayEventSource {
    public event Action<string, DateTime> OnSongPlayed = delegate { };

    public BsUtilsEventSource(IClock clock) {
      _clock = clock;
      BSEvents.levelSelected += PreserveSelectedLevel;
      BSEvents.gameSceneLoaded += RecordStartTime;
      BSEvents.LevelFinished += DispatchIfLongEnough;
    }

    public void Dispose() {
      BSEvents.LevelFinished -= DispatchIfLongEnough;
      BSEvents.gameSceneLoaded -= RecordStartTime;
      BSEvents.levelSelected -= PreserveSelectedLevel;
    }

    private readonly IClock _clock;
    private string _selectedLevelId = "";
    private float _songDuration;
    private DateTime _startTime;

    private void PreserveSelectedLevel(LevelCollectionViewController arg1, IPreviewBeatmapLevel level) {
      _selectedLevelId = level.levelID;
      _songDuration = level.songDuration;
    }

    private void RecordStartTime() {
      _startTime = _clock.Now;
    }

    private void DispatchIfLongEnough(object sender, LevelFinishedEventArgs finished) {
      if (finished is not LevelFinishedWithResultsEventArgs) {
        return;
      }

      DateTime now = _clock.Now;
      TimeSpan duration = now - _startTime;
      bool isPlayedTooShort = duration.Seconds < 10 && _songDuration < 10;
      if (isPlayedTooShort) {
        return;
      }

      OnSongPlayed(_selectedLevelId, now);
    }
  }
}
