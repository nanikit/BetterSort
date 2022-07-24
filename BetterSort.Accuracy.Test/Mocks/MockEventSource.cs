namespace BetterSort.LastPlayed.Test.Mocks {
  using BetterSort.LastPlayed.External;
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
