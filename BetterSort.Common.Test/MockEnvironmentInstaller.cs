using BetterSort.Common.Test.Mocks;
using IPA.Logging;
using SiraUtil.Logging;
using Zenject;

namespace BetterSort.Common.Test {

  public class MockEnvironmentInstaller : Installer {
    private readonly MockLogger _logger;

    public MockEnvironmentInstaller() {
      _logger = new MockLogger();
    }

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<Logger>().FromInstance(_logger).AsSingle();
      Container.BindInterfacesAndSelfTo<SiraLog>().FromInstance(new MockSiraLog(_logger)).AsSingle();
      Container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockBeatleader>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockScoresaber>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockTransformerPluginHelper>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockSongSelection>().AsSingle();
    }
  }
}
