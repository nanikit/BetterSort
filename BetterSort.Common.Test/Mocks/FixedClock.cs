using BetterSort.Common.External;
using System;

namespace BetterSort.Common.Test.Mocks {

  public class FixedClock : IClock {
    public DateTime Now { get; set; }
  }
}
