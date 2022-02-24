using IPA;
using IPALogger = IPA.Logging.Logger;

namespace BsListLastPlayedSort {
  [Plugin(RuntimeOptions.SingleStartInit)]
  public class Plugin {
    internal static Plugin Instance { get; private set; }
    internal static IPALogger Log { get; private set; }

    [Init]
    /// <summary>
    /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
    /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
    /// Only use [Init] with one Constructor.
    /// </summary>
    public void Setup(IPALogger logger) {
      Instance = this;
      Log = logger;
      Log.Debug("Setup.");
    }

    [OnStart]
    public void Initialize() {
      Log.Info("Initialized.");
    }

    [OnExit]
    public void OnExit() {
      // No op, just for suppressing BSIPA confirm.
    }
  }
}
