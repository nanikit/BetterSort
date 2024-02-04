using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using BetterSort.LastPlayed.Test.Mocks;
using BetterSort.Test.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  public class HistoryImporterTest {
    private static readonly string _dataPath = @"UserData";

    private static readonly string _testSphFile = @"{
""custom_level_5AF29356A4F8591D23215F0BACDC6C4D660EF1D0___4___Standard"": [
  {
    ""Date"": 1649863230123,
    ""ModifiedScore"": 1195860,
    ""RawScore"": 1195860,
    ""LastNote"": -1,
    ""Param"": 2
  },
  {
    ""Date"": 1649950920391,
    ""ModifiedScore"": 1197658,
    ""RawScore"": 1197658,
    ""LastNote"": -1,
    ""Param"": 2
  },
  {
    ""Date"": 1650035029964,
    ""ModifiedScore"": 1232244,
    ""RawScore"": 1232244,
    ""LastNote"": -1,
    ""Param"": 2
  }
]}";

    private static readonly string _testSphPath = @"UserData\SongPlayData.json";

    private readonly InMemoryDateRepository _ourHistory;

    private readonly ImmigrationRepository _repository;

    public HistoryImporterTest(ITestOutputHelper output) {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>(new object[] { output });
      container.BindInterfacesAndSelfTo<InMemoryDateRepository>().AsSingle();

      container.Install<SorterInstaller>();

      _ourHistory = container.Resolve<InMemoryDateRepository>();
      _repository = container.Resolve<ImmigrationRepository>();
    }

    [Fact]
    public void TestBothExist() {
      if (Plugin.IsUnityPlayer) {
        // It can overwrite user data. Skip.
        return;
      }

      _ourHistory.LastPlayedDate = new();
      Directory.CreateDirectory(_dataPath);
      File.WriteAllText(_testSphPath, _testSphFile);
      try {
        var data = _repository.Load();
        Assert.Equal(0, data?.LastPlays?.Count);
      }
      finally {
        File.Delete(_testSphPath);
      }
    }

    [Fact]
    public void TestCompleteEmpty() {
      _ourHistory.LastPlayedDate = null;
      var data = _repository.Load();
      Assert.Null(data);
    }

    [Fact]
    public void TestImport() {
      if (Plugin.IsUnityPlayer) {
        // It can overwrite user data. Skip.
        return;
      }

      _ourHistory.LastPlayedDate = null;
      Directory.CreateDirectory(_dataPath);
      File.WriteAllText(_testSphPath, _testSphFile);
      try {
        var data = _repository.Load();
        Assert.Equal(1, data?.LastPlays?.Count);
        var expectation = DateTimeOffset.FromUnixTimeMilliseconds(1650035029964).DateTime;
        Assert.Equal(expectation, data?.LastPlays?["custom_level_5AF29356A4F8591D23215F0BACDC6C4D660EF1D0"]);
      }
      finally {
        File.Delete(_testSphPath);
      }
    }

    [Fact]
    public void TestOurHistory() {
      _ourHistory.LastPlayedDate = new();
      var data = _repository.Load();
      Assert.Equal(0, data?.LastPlays?.Count);
    }
  }
}
