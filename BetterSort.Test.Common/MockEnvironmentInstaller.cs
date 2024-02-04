using BetterSort.Test.Common.Mocks;
using IPA.Logging;
using SiraUtil.Logging;
using Xunit.Abstractions;
using Zenject;

namespace BetterSort.Test.Common {

  public class MockEnvironmentInstaller : Installer {
    private readonly MockLogger _logger;

    public MockEnvironmentInstaller(ITestOutputHelper output) {
      _logger = new MockLogger(output);
    }

    public override void InstallBindings() {
      Container.BindInterfacesAndSelfTo<Logger>().FromInstance(_logger).AsSingle();
      Container.BindInterfacesAndSelfTo<SiraLog>().FromInstance(new MockSiraLog(_logger)).AsSingle();
      Container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockBeatleader>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockScoresaber>().AsSingle();
      Container.BindInterfacesAndSelfTo<MockTransformerPluginHelper>().AsSingle();
    }
  }
}
