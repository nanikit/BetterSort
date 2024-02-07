using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using BetterSort.LastPlayed.Test.Mocks;
using BetterSort.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class HistoryImporterTest {
    private static readonly string _dataPath = @"UserData";

    private static readonly string _testSphFile = """
{"custom_level_5AF29356A4F8591D23215F0BACDC6C4D660EF1D0___4___Standard": [
  {
    "Date": 1649863230123,
    "ModifiedScore": 1195860,
    "RawScore": 1195860,
    "LastNote": -1,
    "Param": 2
  },
  {
    "Date": 1649950920391,
    "ModifiedScore": 1197658,
    "RawScore": 1197658,
    "LastNote": -1,
    "Param": 2
  },
  {
    "Date": 1650035029964,
    "ModifiedScore": 1232244,
    "RawScore": 1232244,
    "LastNote": -1,
    "Param": 2
  }
]}
""";

    private static readonly string _testSphPath = @"UserData\SongPlayData.json";

    private readonly InMemoryPlayedDateJsonRepository _ourHistory;

    private readonly ImmigrationRepository _repository;

    public HistoryImporterTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<InMemoryPlayedDateJsonRepository>().AsSingle();

      container.Install<SorterInstaller>();

      _ourHistory = container.Resolve<InMemoryPlayedDateJsonRepository>();
      _repository = container.Resolve<ImmigrationRepository>();
    }

    [TestMethod]
    public void TestBothExist() {
      if (Plugin.IsUnityPlayer) {
        // It can overwrite user data. Skip.
        return;
      }

      _ourHistory.Json = "{}";
      Directory.CreateDirectory(_dataPath);
      File.WriteAllText(_testSphPath, _testSphFile);
      try {
        var data = _repository.Load();
        Assert.AreEqual(0, data?.LatestRecords?.Count);
      }
      finally {
        File.Delete(_testSphPath);
      }
    }

    [TestMethod]
    public void TestCompleteEmpty() {
      _ourHistory.Json = null;
      var data = _repository.Load();
      Assert.IsNull(data);
    }

    [TestMethod]
    public void TestImport() {
      if (Plugin.IsUnityPlayer) {
        // It can overwrite user data. Skip.
        return;
      }

      _ourHistory.Json = null;
      Directory.CreateDirectory(_dataPath);
      File.WriteAllText(_testSphPath, _testSphFile);
      try {
        var data = _repository.Load();
        Assert.AreEqual(1, data?.LatestRecords?.Count);
        var expectation = DateTimeOffset.FromUnixTimeMilliseconds(1650035029964).DateTime;
        Assert.AreEqual(expectation, data?.LatestRecords?[0].Time);
      }
      finally {
        File.Delete(_testSphPath);
      }
    }

    [TestMethod]
    public void TestOurHistory() {
      _ourHistory.Json = "{}";
      var data = _repository.Load();
      Assert.AreEqual(null, data?.LatestRecords?.Count);
    }
  }
}
