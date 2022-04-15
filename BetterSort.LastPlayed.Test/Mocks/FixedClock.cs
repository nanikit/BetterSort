namespace BetterSort.LastPlayed.Test.Mocks {
  using BetterSort.LastPlayed.External;
  using System;

  internal class FixedClock : IClock {
    public DateTime Now { get; set; }
  }
}
