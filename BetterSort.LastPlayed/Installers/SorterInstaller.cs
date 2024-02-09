using BetterSort.Common.Compatibility;
using BetterSort.Common.Flows;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Sorter;
using Zenject;

namespace BetterSort.LastPlayed.Installers {

  public class SorterInstaller : Installer {

    public override void InstallBindings() {
      Container.Bind<PlayedDateRepository>().AsSingle();

      Container.BindInterfacesAndSelfTo<LastPlayedDateSorter>().AsSingle();
      Container.BindInterfacesAndSelfTo<DifficultySelectingSorter>().AsSingle().WhenInjectedInto<FilterSortAdaptor>();
      Container.BindInterfacesAndSelfTo<FilterSortAdaptor>().AsSingle();
      Container.BindInterfacesAndSelfTo<SorterEnvironment>().AsSingle();
    }
  }
}
