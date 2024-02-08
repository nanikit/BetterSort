using BetterSort.Common.Models;
using BetterSort.Common.Test;
using BetterSort.Common.Test.Mocks;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using IPA.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class HistoryImportTest {

    private static readonly string _testHistoryFile = """
{
  "latestRecords": [
    { "time": "2022-04-01T12:00:00Z", "levelId": "custom_level_111110000000091D23215F0BACDC6C4D660EF1D0" }
  ]
}
""";

    private static readonly List<LastPlayRecord> _testHistory = [
      new (DateTime.Parse("2022-04-01T12:00:00Z").ToUniversalTime(), "custom_level_111110000000091D23215F0BACDC6C4D660EF1D0")
    ];

    private static readonly string _testSphFile1 = """
{"custom_level_5AF29356A4F8591D23215F0BACDC6C4D660EF1D0___4___Standard": [
  {
    "Date": 1649863230123,
    "ModifiedScore": 1195860,
    "RawScore": 1195860,
    "LastNote": -1,
    "Param": 2
  }
]}
""";

    private static readonly string _testSphFile2 = """
{"custom_level_5909C4CC3F242632BF4C826305732489F8C54F9B___3___Standard": [
  {
    "Date": 1654368907388,
    "ModifiedScore": 651188,
    "RawScore": 651188,
    "LastNote": -1,
    "Param": 2
  }
],
"custom_level_558FD3C73D5F7E5BBBF1616760E099EBCFD75903___3___Standard": [
  {
    "Date": 1654369133470,
    "ModifiedScore": 989039,
    "RawScore": 989039,
    "LastNote": -1,
    "Param": 2
  }
]}
""";

    private static readonly string _testSphFile3 = """
{"custom_level_8EAD9FC51CFB027CEF489F1BDC629A3CE0384CA9___0___Standard": [
  {
    "Date": 1668356816205,
    "ModifiedScore": 336441,
    "RawScore": 672882,
    "LastNote": -1,
    "Param": 2
  },
  {
    "Date": 1705325739302,
    "ModifiedScore": 757148,
    "RawScore": 757148,
    "LastNote": -1,
    "Param": 0
  },
  {
    "Date": 1701354482759,
    "ModifiedScore": 662399,
    "RawScore": 662399,
    "LastNote": 1382,
    "Param": 2
  }
]}
""";

    private readonly Mock<IHistoryJsonRepository> _mockJson;
    private readonly PlayedDateRepository _repository;
    private readonly MockLogger _logger;

    public HistoryImportTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      var mockJsonRepository = new Mock<IHistoryJsonRepository>();
      container.Bind<IHistoryJsonRepository>().FromInstance(mockJsonRepository.Object).AsSingle();

      container.Install<SorterInstaller>();

      _mockJson = mockJsonRepository;
      _repository = container.Resolve<PlayedDateRepository>();
      _logger = (container.Resolve<Logger>() as MockLogger)!;
    }

    [TestMethod]
    public void TestCompleteEmpty() {
      _mockJson.Reset();
      _mockJson.Setup(repository => repository.Load()).Returns(() => null);
      _mockJson.Setup(repository => repository.LoadPlayHistory()).Returns(() => null);

      var data = _repository.Load();

      Assert.IsNull(data);
    }

    [TestMethod]
    public void TestOurHistory() {
      _mockJson.Reset();
      _mockJson.Setup(repository => repository.Load()).Returns(_testHistoryFile);
      _mockJson.Setup(repository => repository.LoadPlayHistory()).Returns(() => null);

      var data = _repository.Load();

      CollectionAssert.AreEqual(_testHistory, data?.LatestRecords.ToList());
    }

    [TestMethod]
    public void TestImport() {
      _mockJson.Reset();
      _mockJson.Setup(repository => repository.Load()).Returns(() => null);
      _mockJson.Setup(repository => repository.LoadPlayHistory()).Returns(_testSphFile1);

      var data = _repository.Load();

      var time = DateTimeOffset.FromUnixTimeMilliseconds(1649863230123).DateTime;
      var sphRecord = new List<LastPlayRecord>() {
        new(time, "custom_level_5AF29356A4F8591D23215F0BACDC6C4D660EF1D0", new("Standard", RecordDifficulty.ExpertPlus))
      };
      CollectionAssert.AreEqual(sphRecord, data?.LatestRecords.ToList());
    }

    [TestMethod]
    public void TestBothExist() {
      _mockJson.Reset();
      _mockJson.Setup(repository => repository.Load()).Returns(_testHistoryFile);
      _mockJson.Setup(repository => repository.LoadPlayHistory()).Returns(_testSphFile2);

      var data = _repository.Load();

      CollectionAssert.AreEqual(_testHistory, data?.LatestRecords.ToList());
    }

    [TestMethod]
    public void TestBackupWhenError() {
      _mockJson.Reset();
      string corruptedData = "corrupted data";
      _mockJson.Setup(repository => repository.Load()).Returns(corruptedData);
      _mockJson.Setup(repository => repository.LoadPlayHistory()).Returns(() => null);

      var data = _repository.Load();

      Assert.IsNull(data);
      _mockJson.Verify(repository => repository.SaveBackup(corruptedData), Times.Once);
      _logger.Logs.Any(log => log.Level == Logger.Level.Error);
    }

    [TestMethod]
    public void TestSameMapPlay() {
      var (records, message) = PlayedDateRepository.ConvertSongPlayHistory(_testSphFile3);

      var time = DateTimeOffset.FromUnixTimeMilliseconds(1705325739302).DateTime;
      var sphRecord = new List<LastPlayRecord>() {
        new(time, "custom_level_8EAD9FC51CFB027CEF489F1BDC629A3CE0384CA9", new("Standard", RecordDifficulty.Easy))
      };
      CollectionAssert.AreEqual(sphRecord, records);
      Assert.IsNull(message);
    }
  }
}
