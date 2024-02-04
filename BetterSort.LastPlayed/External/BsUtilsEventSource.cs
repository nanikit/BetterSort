using BetterSort.Common.External;
using BS_Utils.Utilities;
using SiraUtil.Logging;
using System;

namespace BetterSort.LastPlayed.External {

  public interface IPlayEventSource : IDisposable {

    event Action<string, DateTime> OnSongPlayed;
  }

  internal class BsUtilsEventSource : IPlayEventSource {
    private readonly IClock _clock;

    private readonly SiraLog _logger;

    private readonly Scoresaber _scoresaber;

    private readonly Beatleader _beatleader;

    private string _selectedLevelId = "";

    private string _selectedSongName = "";

    private float _songDuration;

    private DateTime _startTime;

    public BsUtilsEventSource(IClock clock, SiraLog logger, Scoresaber scoresaber, Beatleader beatleader) {
      _clock = clock;
      _logger = logger;
      _scoresaber = scoresaber;
      _beatleader = beatleader;
      BSEvents.levelSelected += PreserveSelectedLevel;
      BSEvents.gameSceneLoaded += RecordStartTime;
      BSEvents.LevelFinished += DispatchIfLongEnough;
    }

    public event Action<string, DateTime> OnSongPlayed = delegate { };

    public void Dispose() {
      BSEvents.LevelFinished -= DispatchIfLongEnough;
      BSEvents.gameSceneLoaded -= RecordStartTime;
      BSEvents.levelSelected -= PreserveSelectedLevel;
    }

    private void PreserveSelectedLevel(LevelCollectionViewController arg1, IPreviewBeatmapLevel level) {
      _selectedLevelId = level.levelID;
      _selectedSongName = level.songName;
      _songDuration = level.songDuration;
    }

    private void RecordStartTime() {
      _startTime = _clock.Now;
    }

    private void DispatchIfLongEnough(object sender, LevelFinishedEventArgs finished) {
      if (_selectedLevelId == "") {
        _logger.Warn("Cannot determine selected level.");
        return;
      }
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

      var now = _clock.Now;
      var duration = now - _startTime;
      bool isPlayedTooShort = duration.TotalSeconds < 10 && _songDuration > 10;
      if (isPlayedTooShort) {
        _logger.Info($"Skip record due to too short play: {_selectedSongName}");
        return;
      }

      _logger.Info($"Dispatch play event: {_selectedSongName}");
      OnSongPlayed(_selectedLevelId, now);
    }
  }
}
