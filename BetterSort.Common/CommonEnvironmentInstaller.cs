using BetterSort.Common.Compatibility;
using BetterSort.Common.External;
using Zenject;

namespace BetterSort.Common {

  public class CommonEnvironmentInstaller : Installer {

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<Clock>().AsSingle();
      Container.BindInterfacesAndSelfTo<Beatleader>().AsSingle();
      Container.BindInterfacesAndSelfTo<Scoresaber>().AsSingle();
      Container.BindInterfacesAndSelfTo<TransformerPluginHelper>().AsSingle();
      Container.BindInterfacesAndSelfTo<SongSelection>().AsSingle();
    }
  }
}
