using BetterSort.Accuracy.External;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {

  internal class MockId : ILeaderboardId {
    public static readonly string QuitUserId = "76561198387870564";

    public Task<string?> GetUserId() {
      return Task.FromResult<string?>(QuitUserId);
    }
  }
}
