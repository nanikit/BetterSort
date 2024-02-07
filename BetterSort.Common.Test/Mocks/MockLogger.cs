using IPA.Logging;
using SiraUtil.Logging;

namespace BetterSort.Common.Test.Mocks {

  public class MockSiraLog : SiraLog {

    public MockSiraLog(Logger logger) {
      typeof(SiraLog).GetProperty(nameof(Logger)).SetValue(this, logger);
    }
  }

  public class MockLogger : Logger {

    public override void Log(Level level, string message) {
      System.Diagnostics.Trace.WriteLine($"[{level}] {message}");
    }
  }
}
