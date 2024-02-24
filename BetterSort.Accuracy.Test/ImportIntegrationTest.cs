using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace BetterSort.Accuracy.Test {

  [TestCategory("NoCi")]
  [TestClass]
  public class ImportIntegrationTest {
    private readonly UnifiedImporter _importer;
    private readonly MockProgress _progress;
    private readonly InMemoryJsonRepository _repository;

    public ImportIntegrationTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<FakeHttpService>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryJsonRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<MockId>().AsSingle();
      container.BindInterfacesAndSelfTo<MockProgress>().AsSingle();

      container.Install<SorterInstaller>();

      _importer = container.Resolve<UnifiedImporter>();
      _progress = container.Resolve<MockProgress>();
      _repository = container.Resolve<InMemoryJsonRepository>();
    }

    [TestMethod]
    public async Task TestImport() {
      Assert.IsNull(_repository.Json);

      await _importer.CollectOrImport().ConfigureAwait(false);

      Assert.IsNotNull(_repository.Json);
      Assert.IsTrue(_progress.Progresses.Count > 1);
      Assert.IsNull(_progress.Progresses.Last()!.Ratio);
    }
  }
}
