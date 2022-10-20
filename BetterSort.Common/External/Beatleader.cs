namespace BetterSort.Common.External {
  using IPA.Loader;
  using System;
  using System.Reflection;
  using IPALogger = IPA.Logging.Logger;

  public class Beatleader {
    public Beatleader(IPALogger logger) {
      _logger = logger;

      var beatleader = (PluginMetadata?)PluginManager.GetPluginFromId("BeatLeader");
      if (beatleader == null) {
        _logger.Info("BeatLeader is not detected.");
        return;
      }

      string typeName = "BeatLeader.Replayer.ReplayerLauncher";
      var prop = beatleader?.Assembly.GetType(typeName)?
        .GetProperty("IsStartedAsReplay", BindingFlags.Static | BindingFlags.Public);
      if (prop == null) {
        _logger.Warn("BeatLeader replay detection hook failure");
        return;
      }

      _replayStarted = prop;
    }

    public bool IsInReplay() {
      try {
        return _replayStarted != null && (bool)_replayStarted.GetValue(null, null);
      }
      catch (Exception exception) {
        _logger.Debug($"BeatLeader hook exception: {exception}");
      }
      return false;
    }

    private readonly PropertyInfo? _replayStarted;
    private readonly IPALogger _logger;
  }
}
