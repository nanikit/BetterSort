using IPA.Logging;
using SiraUtil.Logging;
using Xunit.Abstractions;

namespace BetterSort.Test.Common.Mocks {

  public class MockSiraLog : SiraLog {

    public MockSiraLog(Logger logger) {
      typeof(SiraLog).GetProperty(nameof(Logger)).SetValue(this, logger);
    }
  }

  public class MockLogger : Logger {
    private readonly ITestOutputHelper _output;

    public MockLogger(ITestOutputHelper output) {
      _output = output;
    }

    public override void Log(Level level, string message) {
      _output.WriteLine($"[{level}] {message}");
    }
  }
}
