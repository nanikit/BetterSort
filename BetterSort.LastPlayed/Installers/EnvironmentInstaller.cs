using BetterSort.LastPlayed.External;
using Zenject;

namespace BetterSort.LastPlayed.Installers {

  internal class EnvironmentInstaller : Installer {

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<BsUtilsEventSource>().AsSingle();
      Container.BindInterfacesAndSelfTo<HistoryJsonRepository>().AsSingle();
    }
  }
}
