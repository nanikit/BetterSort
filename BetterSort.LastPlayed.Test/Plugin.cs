using IPA;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.LastPlayed.Test {

  [Plugin(RuntimeOptions.SingleStartInit)]
  public class Plugin {
    internal static bool IsUnityPlayer { get; set; } = true;
    internal static IPALogger? Logger { get; private set; }

    [Init]
    public void Init(IPALogger logger) {
      Logger = logger;
    }

    [OnExit]
    public void OnApplicationQuit() {
      Logger?.Info("Test end.");
    }

    [OnStart]
    public void OnApplicationStart() {
      Logger?.Info("Test start.");
      //new TestRunner(Logger).Test(new List<Assembly> { typeof(Plugin).Assembly });
      if (IsUnityPlayer) {
        Application.Quit();
      }
    }
  }
}
