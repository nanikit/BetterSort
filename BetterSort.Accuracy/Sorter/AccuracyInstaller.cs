using BetterSort.Accuracy.External;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {

  using BetterSort.Common.External;
  using IPA.Logging;

  public class AccuracyInstaller : Installer {
    private readonly IPALogger _logger;

    public AccuracyInstaller(IPALogger logger) {
      _logger = logger;
    }

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<LeaderboardId>().AsSingle();
      Container.Bind<ScoreImporterHelper>().AsSingle();
      Container.BindInterfacesAndSelfTo<ScoresaberImporter>().AsSingle();
      Container.BindInterfacesAndSelfTo<BeatLeaderImporter>().AsSingle();
      Container.Bind<UnifiedImporter>().AsSingle();

      Container.Bind<Beatleader>().FromInstance(new Beatleader(_logger.GetChildLogger("BetterSort.Accuracy.Beatleader"))).AsSingle();
      Container.Bind<Scoresaber>().FromInstance(new Scoresaber(_logger.GetChildLogger("BetterSort.Accuracy.Scoresaber"))).AsSingle();
      Container.BindInterfacesAndSelfTo<BsUtilsInterop>().AsSingle();

      Container.BindInterfacesAndSelfTo<AccuracySorter>().AsSingle();
      Container.BindInterfacesAndSelfTo<UIAwareSorter>().AsSingle();
      Container.Bind<SorterEnvironment>().AsSingle().NonLazy();
    }
  }
}
