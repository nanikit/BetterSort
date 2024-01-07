namespace BetterSort.Test.Common {

  using Xunit.Abstractions;
  using IPALogger = IPA.Logging.Logger;

  public class TestLogger : IPALogger {
    private readonly ITestOutputHelper _output;

    public TestLogger(ITestOutputHelper output) {
      _output = output;
    }

    public override void Log(Level level, string message) {
      _output.WriteLine($"[{level}]: {message}");
    }
  }

  internal class XUnitLogger : ITestOutputHelper {

    public void WriteLine(string message) {
    }

    public void WriteLine(string format, params object[] args) {
    }
  }
}
