using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using Zenject;

namespace BetterSort.Accuracy.Sorter {
  using BetterSort.Common.External;

  public class AccuracyInstaller : Installer {
    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<LeaderboardId>().AsSingle();
      Container.Bind<ScoreImporterHelper>().AsSingle();
      Container.BindInterfacesAndSelfTo<ScoresaberImporter>().AsSingle();
      Container.BindInterfacesAndSelfTo<BeatLeaderImporter>().AsSingle();
      Container.Bind<UnifiedImporter>().AsSingle();

      Container.Bind<Beatleader>().AsSingle();
      Container.Bind<Scoresaber>().AsSingle();
      Container.BindInterfacesAndSelfTo<BsUtilsEventSource>().AsSingle();

      Container.BindInterfacesAndSelfTo<AccuracySorter>().AsSingle();
      Container.Bind<FilterSortAdaptor>().AsSingle();
      Container.Bind<SorterEnvironment>().AsSingle().NonLazy();
    }
  }
}
