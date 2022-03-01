namespace BetterSongList.LastPlayedSort.Test.Mocks {
  using BetterSongList.LastPlayedSort.External;
  using System;

  internal class FixedClock : IClock {
    public DateTime Now { get; set; }
  }
}
