using System;

namespace BetterSort.Common.External {
  public interface IClock {
    DateTime Now { get; }
  }

  public class Clock : IClock {
    public DateTime Now => DateTime.Now;
  }
}
