namespace BetterSort.Accuracy.Test {

  using BetterSort.Test.Common;
  using IPA;
  using System.Collections.Generic;
  using System.Reflection;
  using UnityEngine;
  using IPALogger = IPA.Logging.Logger;

  [Plugin(RuntimeOptions.SingleStartInit)]
  public class Plugin {
    internal static IPALogger? Logger { get; private set; }

    internal static bool IsUnityPlayer { get; set; } = true;

    /// <summary>
    /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
    /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
    /// Only use [Init] with one Constructor.
    /// </summary>
    [Init]
    public void Init(IPALogger logger) {
      Logger = logger;
    }

    [OnStart]
    public void OnApplicationStart() {
      Logger?.Info("Test start.");
      new TestRunner(Logger).Test(new List<Assembly> { typeof(Plugin).Assembly });
      if (IsUnityPlayer) {
        Application.Quit();
      }
    }

    [OnExit]
    public void OnApplicationQuit() {
      Logger?.Info("Test end.");
    }
  }
}
