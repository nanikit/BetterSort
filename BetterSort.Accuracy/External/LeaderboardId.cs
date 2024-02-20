using BS_Utils.Gameplay;
using SiraUtil.Logging;
using System;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public interface ILeaderboardId {

    Task<string?> GetUserId();
  }

  public class LeaderboardId(SiraLog logger) : ILeaderboardId {

    public async Task<string?> GetUserId() {
      var user = await GetUserInfo.GetUserAsync().ConfigureAwait(false);
      if (user.platform != UserInfo.Platform.Steam) {
        string platform = Enum.GetName(typeof(UserInfo.Platform), user.platform);
        logger.Warn($"User platform is {platform}, importing is not tested.");
      }
      return user.platformUserId;
    }
  }
}
