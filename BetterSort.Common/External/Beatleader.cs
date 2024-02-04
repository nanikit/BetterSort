using IPA.Loader;
using SiraUtil.Logging;
using System;
using System.Reflection;

namespace BetterSort.Common.External {

  public class Beatleader {
    private readonly SiraLog _logger;

    private readonly PropertyInfo? _replayStarted;

    public Beatleader(SiraLog logger) {
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
        _logger.Warn($"BeatLeader hook exception: {exception}");
      }
      return false;
    }
  }
}
