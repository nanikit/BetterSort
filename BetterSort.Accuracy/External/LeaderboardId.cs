namespace BetterSort.Accuracy.External {
  using BS_Utils.Gameplay;
  using Steamworks;
  using System;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public interface ILeaderboardId {
    Task<string?> GetUserId();
  }

  public class LeaderboardId : ILeaderboardId {
    public LeaderboardId(IPALogger logger) {
      _logger = logger;
    }

    private readonly IPALogger _logger;
    private string? _id;

    public async Task<string?> GetUserId() {
      if (_id != null) {
        return _id;
      }

      // Link error occurs by method unit. So guard by separating method.
      try {
        _id = await GetPlatformId().ConfigureAwait(false);
        return _id;
      }
      catch (Exception exception) {
        _logger.Error(exception);
        _logger.Info("Failed to get platform ID. Try steam fallback.");
      }
      // Try workaround if missing bs utils.
      try {
        _id = GetSteamId();
        return _id;
      }
      catch (Exception exception) {
        _logger.Error(exception);
        _logger.Info("Failed to get steam ID. Give up importing accuracy from online.");
      }
      return null;
    }

    private async Task<string?> GetPlatformId() {
      var user = await GetUserInfo.GetUserAsync().ConfigureAwait(false);
      return user?.platformUserId;
    }

    private string? GetSteamId() {
      if (SteamManager.Initialized) {
        var id = SteamUser.GetSteamID();
        return id.ToString();
      }
      return null;
    }
  }
}
