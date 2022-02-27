namespace BetterSongList.LastPlayedSort.Test.Mocks {
  using BetterSongList.LastPlayedSort.External;
  using System;

  internal class FixedClock : IClock {
    public FixedClock(DateTime now) {
      Now = now;
    }

    public DateTime Now { get; private set; }
  }
}
