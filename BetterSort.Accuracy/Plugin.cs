using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Installers;
using BetterSort.Common;
using HarmonyLib;
using IPA;
using SiraUtil.Zenject;
using System.Runtime.CompilerServices;
using IPALogger = IPA.Logging.Logger;

[assembly: InternalsVisibleTo("BetterSort.Accuracy.Test")]

namespace BetterSort.Accuracy {

  [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
  public class Plugin {

    [Init]
    public Plugin(IPALogger logger, Zenjector zenjector) {
      logger.Debug("Initialize()");

      zenjector.UseHttpService();
      zenjector.UseLogger(logger);
      zenjector.Install<CommonEnvironmentInstaller>(Location.App);
      zenjector.Install<EnvironmentInstaller>(Location.App);
      zenjector.Install(Location.App, container => {
        container.Bind<Harmony>()
          .WithId("BetterSort.Accuracy.Harmony")
          .FromInstance(new Harmony("BetterSort.Accuracy"))
          .AsCached();
      });
      zenjector.Install<SorterInstaller>(Location.App);

      logger.Info("Initialized.");
    }
  }
}
