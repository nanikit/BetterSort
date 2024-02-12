using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Sorter;
using BetterSort.Common.Compatibility;
using BetterSort.Common.Flows;
using Zenject;

namespace BetterSort.Accuracy.Installers {

  public class SorterInstaller : Installer {

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<ScoreImporterHelper>().AsSingle();
      Container.BindInterfacesAndSelfTo<ScoresaberImporter>().AsSingle();
      Container.BindInterfacesAndSelfTo<BeatLeaderImporter>().AsSingle();
      Container.Bind<UnifiedImporter>().AsSingle();
      Container.BindInterfacesAndSelfTo<AccuracyRepository>().AsSingle();

      Container.Bind<AccuracySorter>().AsSingle();
      Container.BindInterfacesAndSelfTo<UIAwareSorter>().AsSingle();
      Container.BindInterfacesAndSelfTo<DifficultySelectingSorter>().AsSingle().WhenInjectedInto<FilterSortAdaptor>();
      Container.BindInterfacesAndSelfTo<FilterSortAdaptor>().AsSingle();
      Container.BindInterfacesAndSelfTo<SorterEnvironment>().AsSingle().NonLazy();
    }
  }
}
