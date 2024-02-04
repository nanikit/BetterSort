using IPA.Loader;
using System;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Common.External {

  public class Scoresaber {
    private readonly IPALogger _logger;

    private readonly MethodBase? _playbackDisabled;

    public Scoresaber(IPALogger logger) {
      _logger = logger;

      // GetPluginFromId's result can be null but intellisense doesn't catch it.
      var scoresaber = (PluginMetadata?)PluginManager.GetPluginFromId("ScoreSaber");
      if (scoresaber == null) {
        _logger.Info("Scoresaber is not detected.");
        return;
      }

      string typeName = "ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted";
      var method = scoresaber?.Assembly.GetType(typeName)?
        .GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
      if (method == null) {
        _logger.Warn("Scoresaber replay detection hook failure");
        return;
      }
      _playbackDisabled = method;
    }

    public bool IsInReplay() {
      try {
        return _playbackDisabled != null && !(bool)_playbackDisabled.Invoke(null, null);
      }
      catch (Exception exception) {
        _logger.Warn($"Scoresaber hook exception: {exception}");
      }
      return false;
    }
  }
}
