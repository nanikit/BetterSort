namespace BetterSongList.LastPlayedSort.Test.Mocks {
  using BetterSongList.LastPlayedSort.External;
  using System;

  internal class MockEventSource : IPlayEventSource {
    public event Action<string, DateTime> OnSongPlayed = delegate { };

    public void SimulatePlay(string levelId, DateTime instant) {
      OnSongPlayed(levelId, instant);
    }

    public void Dispose() {
    }
  }
}
