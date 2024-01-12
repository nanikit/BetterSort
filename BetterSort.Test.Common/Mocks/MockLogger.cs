using SiraUtil.Logging;
using Xunit.Abstractions;

namespace BetterSort.Test.Common.Mocks {

  public class MockLogger : SiraLog {
    private readonly ITestOutputHelper _output;

    public MockLogger(ITestOutputHelper output) {
      _output = output;
    }
  }
}
