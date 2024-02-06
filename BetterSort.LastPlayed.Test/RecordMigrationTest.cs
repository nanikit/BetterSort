using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using BetterSort.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class RecordMigrationTest {
    private readonly IPlayedDateRepository _repository;
    private readonly Mock<IPlayedDateJsonRepository> _mock;

    public RecordMigrationTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();

      _mock = new Mock<IPlayedDateJsonRepository>();
      container.Bind<IPlayedDateJsonRepository>().FromInstance(_mock.Object);
      container.BindInterfacesAndSelfTo<PlayedDateRepository>().AsSingle();

      container.Install<SorterInstaller>();

      _repository = container.Resolve<IPlayedDateRepository>();
    }

    [TestMethod]
    public void TestMigration() {
      _mock.Setup(library => library.Load())
          .Returns("""
{ "lastPlays": { "custom_level_6B6C6FD5204104B70D56E7E43CEEDABD5319D187": "2024-02-06T07:45:42.521672+09:00" } }
""");
      var records = _repository.Load();
      Assert.AreEqual(1, records?.LastPlays?.Count);
    }
  }
}
