using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void TestBeatLeader() {
      var beatLeader = new BeatleaderBest();
      string url = beatLeader.GetRecordUrl(MockId.QuitUserId, 1);

      Assert.AreEqual(url, $"https://api.beatleader.xyz/player/{MockId.QuitUserId}/scores?page=1&sortBy=date&order=desc");

      string json = File.ReadAllText(@"..\..\Data\beatleader_scores.json");
      var (Records, Paging, Log) = beatLeader.ToBestRecords(json);

      Assert.AreEqual(8, Records.Count);
      Assert.IsTrue(0 < Records[0].Accuracy && Records[0].Accuracy <= 1);
      Assert.AreEqual(1849, Paging.Total);
      Assert.IsNull(Log);
    }

    [TestMethod]
    public async Task TestImport() {
      Directory.CreateDirectory("UserData");
      var records = await _container.Resolve<UnifiedImporter>().CollectRecordsFromOnline().ConfigureAwait(false);
      await _container.Resolve<AccuracyRepository>().Save(records).ConfigureAwait(false);
    }

    [TestMethod]
    public void TestScoresaber() {
      var scoresaber = new ScoresaberBest();
      string url = scoresaber.GetRecordUrl(MockId.QuitUserId, 1);

      Assert.AreEqual(url, $"https://scoresaber.com/api/player/{MockId.QuitUserId}/scores?page=1&sort=recent");

      string json = File.ReadAllText(@"..\..\Data\scoresaber_scores.json");
      var (Records, Paging, Log) = scoresaber.ToBestRecords(json);

      Assert.AreEqual(8, Records.Count);
      Assert.IsTrue(0 < Records[0].Accuracy && Records[0].Accuracy <= 1);
      Assert.AreEqual(2133, Paging.Total);
      Assert.IsNull(Log);
    }
  }

  internal class MockId : ILeaderboardId {
    public static readonly string QuitUserId = "76561198387870564";

    public Task<string?> GetUserId() {
      return Task.FromResult<string?>(QuitUserId);
    }
  }
}
