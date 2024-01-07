using BetterSort.Common.External;
using System;

namespace BetterSort.Test.Common.Mocks {

  public class FixedClock : IClock {
    public DateTime Now { get; set; }
  }
}
