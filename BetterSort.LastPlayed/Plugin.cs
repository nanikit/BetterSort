using BetterSort.Common;
using BetterSort.LastPlayed.Installers;
using IPA;
using IPA.Logging;
using SiraUtil.Zenject;

namespace BetterSort.LastPlayed {

  [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
  public class Plugin {

    [Init]
    public Plugin(Logger logger, Zenjector zenjector) {
      logger.Debug("Initialize()");

      zenjector.UseHttpService();
      zenjector.UseLogger(logger);
      zenjector.Install<CommonEnvironmentInstaller>(Location.App);
      zenjector.Install<EnvironmentInstaller>(Location.App);
      zenjector.Install<SorterInstaller>(Location.App);

      logger.Info("Initialized.");
    }
  }
}
