using BetterSort.Common.External;
using BetterSort.Common.Test;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  // TODO: convert existing to use static
  // TODO: test backup
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
  }
]}
""";

    private readonly Mock<IHistoryJsonRepository> _mockJson;
    private readonly PlayedDateRepository _repository;

    public HistoryImportTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      var mockJsonRepository = new Mock<IHistoryJsonRepository>();
      container.Bind<IHistoryJsonRepository>().FromInstance(mockJsonRepository.Object).AsSingle();

      container.Install<SorterInstaller>();

      _mockJson = mockJsonRepository;
      _repository = container.Resolve<PlayedDateRepository>();
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
  }
}
