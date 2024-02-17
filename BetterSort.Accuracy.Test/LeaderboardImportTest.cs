using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Zenject;

namespace BetterSort.Accuracy.Test {

  [TestCategory("NoCi")]
  [TestClass]
  public class LeaderboardImportTest {
    private readonly DiContainer _container;

    public LeaderboardImportTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<FakeHttpService>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryJsonRepository>().AsSingle();
      container.Install<SorterInstaller>();

      var mockId = new MockId();
      container.BindInterfacesAndSelfTo<MockId>().FromInstance(mockId);
      _container = container;
    }

    [TestMethod]
    public async Task TestBeatLeader() {
      var beatLeader = _container.Resolve<BeatLeaderImporter>();
      var page = await beatLeader.GetPagedRecord(MockId.QuitUserId, 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.AreNotEqual(0, records.Count);
      Assert.IsTrue(0 <= records[0].Accuracy && records[0].Accuracy <= 1);
      Assert.IsTrue(0 <= maxPage && maxPage <= int.MaxValue);
    }

    [TestMethod]
    public async Task TestImport() {
      Directory.CreateDirectory("UserData");
      var records = await _container.Resolve<UnifiedImporter>().CollectRecordsFromOnline().ConfigureAwait(false);
      await _container.Resolve<AccuracyRepository>().Save(records).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task TestScoresaber() {
      var scoresaber = _container.Resolve<ScoresaberImporter>();
      var page = await scoresaber.GetPagedRecord(MockId.QuitUserId, 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.AreNotEqual(0, records.Count);
      Assert.IsTrue(0 <= records[0].Accuracy && records[0].Accuracy <= 1);
      Assert.IsTrue(0 <= maxPage && maxPage <= int.MaxValue);
    }
  }

  internal class MockId : ILeaderboardId {
    public static readonly string QuitUserId = "76561198387870564";

    public Task<string?> GetUserId() {
      return Task.FromResult<string?>(QuitUserId);
    }
  }
}
