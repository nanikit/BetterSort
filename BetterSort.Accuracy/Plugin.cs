using BetterSort.Accuracy.Installers;
using BetterSort.Common;
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
      zenjector.Install<CommonEnvironmentInstaller>(Location.Menu);
      zenjector.Install<EnvironmentInstaller>(Location.Menu);
      zenjector.Install<SorterInstaller>(Location.Menu);

      logger.Info("Initialized.");
    }
  }
}
