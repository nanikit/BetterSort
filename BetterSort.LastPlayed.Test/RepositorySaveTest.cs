using BetterSort.Common.Models;
using BetterSort.LastPlayed.External;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class RepositorySaveTest {

    [TestMethod]
    public void TestEmptyData() {
      string json = PlayedDateRepository.Serialize([]);

      Assert.AreEqual($$"""
{
  "latestRecords": [],
  "version": "{{typeof(PlayedDateRepository).Assembly.GetName().Version}}"
}
""", json);
    }

    [TestMethod]
    public void TestSingleData() {
      string json = PlayedDateRepository.Serialize([
        new(DateTime.Parse("2024-02-06T07:00:00.000000+09:00"), "custom_level_000", new PlayedMap("Standard", RecordDifficulty.ExpertPlus))
      ]);

      Assert.AreEqual($$"""
{
  "latestRecords": [
    { "time": "2024-02-06T07:00:00+09:00", "levelId": "custom_level_000", "map": { "type": "Standard", "difficulty": "ExpertPlus" } }
  ],
  "version": "{{typeof(PlayedDateRepository).Assembly.GetName().Version}}"
}
""", json);
    }

    [TestMethod]
    public void TestDoubleData() {
      string json = PlayedDateRepository.Serialize([
        new(DateTime.Parse("2024-02-03T07:00:00.000000+09:00"), "custom_level_100", new PlayedMap("Lawless", RecordDifficulty.Easy)),
        new(DateTime.Parse("2024-02-06T07:00:00.000000+09:00"), "custom_level_000", new PlayedMap("Standard", RecordDifficulty.ExpertPlus)),
      ]);

      Assert.AreEqual($$"""
{
  "latestRecords": [
    { "time": "2024-02-06T07:00:00+09:00", "levelId": "custom_level_000", "map": { "type": "Standard", "difficulty": "ExpertPlus" } },
    { "time": "2024-02-03T07:00:00+09:00", "levelId": "custom_level_100", "map": { "type": "Lawless", "difficulty": "Easy" } }
  ],
  "version": "{{typeof(PlayedDateRepository).Assembly.GetName().Version}}"
}
""", json);
    }
  }
}
