using BetterSort.Common.Compatibility;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Sorter;
using Zenject;

namespace BetterSort.LastPlayed.Installers {

  public class SorterInstaller : Installer {

    public override void InstallBindings() {
      Container.Bind<SongPlayHistoryImporter>().AsSingle();
      Container.Bind<ImmigrationRepository>().AsSingle();
      Container.Bind<PlayedDateRepository>().AsSingle();

      Container.BindInterfacesAndSelfTo<FilterSortAdaptor>().AsSingle();
      Container.BindInterfacesAndSelfTo<LastPlayedDateSorter>().AsSingle();
      Container.BindInterfacesAndSelfTo<SorterEnvironment>().AsSingle();
    }
  }
}
