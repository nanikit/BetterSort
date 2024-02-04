using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Test.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zenject;

namespace BetterSort.Accuracy.Test {

  public class LeaderboardImportTest {
    private readonly DiContainer _container;

    public LeaderboardImportTest(ITestOutputHelper output) {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>(new[] { output });
      container.BindInterfacesAndSelfTo<FakeHttpService>().AsSingle();
      container.BindInterfacesAndSelfTo<AccuracyRepository>().AsSingle();
      container.Install<SorterInstaller>();

      var mockId = new MockId();
      container.BindInterfacesAndSelfTo<MockId>().FromInstance(mockId);
      _container = container;
    }

    [Fact]
    public async Task TestBeatLeader() {
      var beatLeader = _container.Resolve<BeatLeaderImporter>();
      var page = await beatLeader.GetPagedRecord(MockId.QuitUserId, 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.NotEmpty(records);
      Assert.InRange(records[0].Accuracy, 0, int.MaxValue);
      Assert.InRange(maxPage, 1, int.MaxValue);
    }

    [Fact]
    public async Task TestImport() {
      Directory.CreateDirectory("UserData");
      var records = await _container.Resolve<UnifiedImporter>().CollectRecordsFromOnline().ConfigureAwait(false);
      await _container.Resolve<AccuracyRepository>().Save(records).ConfigureAwait(false);
    }

    [Fact]
    public async Task TestScoresaber() {
      var scoresaber = _container.Resolve<ScoresaberImporter>();
      var page = await scoresaber.GetPagedRecord(MockId.QuitUserId, 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.NotEmpty(records);
      Assert.InRange(records[0].Accuracy, 0, int.MaxValue);
      Assert.InRange(maxPage, 1, int.MaxValue);
    }
  }

  internal class MockId : ILeaderboardId {
    public static readonly string QuitUserId = "76561198387870564";

    public Task<string?> GetUserId() {
      return Task.FromResult<string?>(QuitUserId);
    }
  }
}
