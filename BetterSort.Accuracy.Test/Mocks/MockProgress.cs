using BetterSort.Accuracy.External;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {

  public class MockProgress : IProgressBar {
    public List<ProgressMessage?> Progresses { get; set; } = [];

    public Task SetMessage(ProgressMessage? message) {
      Progresses.Add(message);
      return Task.CompletedTask;
    }
  }
}
