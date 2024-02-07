namespace BetterSort.LastPlayed.Test.Mocks {

  using BetterSort.LastPlayed.External;
  using System;

  internal class MockEventSource : IPlayEventSource {

    public event Action<LastPlayRecord> OnSongPlayed = delegate { };

    public void Dispose() {
    }

    public void SimulatePlay(LastPlayRecord record) {
      OnSongPlayed(record);
    }
  }
}
