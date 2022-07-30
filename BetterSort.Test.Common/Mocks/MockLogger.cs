namespace BetterSort.Test.Common.Mocks {
  using IPALogger = IPA.Logging.Logger;

  public class MockLogger : IPALogger {
    public override void Log(Level level, string message) {
      System.Console.WriteLine($"[{level}]: {message}");
    }
  }
}
