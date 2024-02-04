using Xunit.Abstractions;

namespace BetterSort.Test.Common {

  internal class XUnitLogger : ITestOutputHelper {

    public void WriteLine(string message) {
    }

    public void WriteLine(string format, params object[] args) {
    }
  }
}
