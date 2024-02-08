using IPA.Logging;
using SiraUtil.Logging;
using System.Collections.Generic;

namespace BetterSort.Common.Test.Mocks {

  public class MockSiraLog : SiraLog {

    public MockSiraLog(Logger logger) {
      typeof(SiraLog).GetProperty(nameof(Logger)).SetValue(this, logger);
    }
  }

  public class MockLogger : Logger {
    public List<(Level Level, string Message)> Logs { get; } = new();

    public override void Log(Level level, string message) {
      Logs.Add((level, message));
      System.Diagnostics.Trace.WriteLine($"[{level}] {message}");
    }
  }
}
