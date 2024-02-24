using BetterSort.Accuracy.External;
using Zenject;

namespace BetterSort.Accuracy.Installers {

  public class EnvironmentInstaller : Installer {

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<LeaderboardId>().AsSingle();
      Container.BindInterfacesAndSelfTo<BsUtilsInterop>().AsSingle();
      Container.BindInterfacesAndSelfTo<AccuracyJsonRepository>().AsSingle();
      Container.BindInterfacesAndSelfTo<ProgressBar>().FromNewComponentOnNewGameObject().AsSingle();
    }
  }
}
