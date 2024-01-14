using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
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
      zenjector.Install(Location.App, container => {
        container.Bind<Harmony>()
          .WithId("BetterSort.Accuracy.Harmony")
          .FromInstance(new Harmony("BetterSort.Accuracy"))
          .AsCached();
        container.BindInterfacesAndSelfTo<Clock>().AsSingle();
        container.BindInterfacesAndSelfTo<AccuracyRepository>().AsSingle();
      });
      zenjector.Install<AccuracyInstaller>(Location.App, logger);

      logger.Info("Initialized.");
    }

    public static bool IsTest { get; set; }
  }
}
