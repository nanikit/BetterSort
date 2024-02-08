using BetterSort.Common.Models;
using BetterSort.LastPlayed.External;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class RecordMigrationTest {

    [TestMethod]
    public void TestEmptyData() {
      var (records, isMigrated) = PlayedDateRepository.MigrateData("{}");

      Assert.AreEqual(new CompatibleData(), records);
      Assert.IsFalse(isMigrated);
    }

    [TestMethod]
    public void TestFreshData() {
      var (records, isMigrated) = PlayedDateRepository.MigrateData("""
{
  "latestRecords": [
    {
      "time": "2022-04-01T12:00:00Z",
      "levelId": "custom_level_111110000000091D23215F0BACDC6C4D660EF1D0",
      "map": { "type": "Standard", "difficulty": "ExpertPlus" }
    }
  ],
  "version": "2.1.0"
}
""");

      CollectionAssert.AreEquivalent(new List<LastPlayRecord> {
        new(
          DateTime.Parse("2022-04-01T12:00:00Z").ToUniversalTime(),
          "custom_level_111110000000091D23215F0BACDC6C4D660EF1D0",
          new PlayedMap("Standard", RecordDifficulty.ExpertPlus)
        )
      }, records.LatestRecords.ToList());
      Assert.AreEqual("2.1.0", records.Version);
      Assert.IsFalse(isMigrated);
    }

    [TestMethod]
    public void TestOldData() {
      var (records, isMigrated) = PlayedDateRepository.MigrateData("""
{
  "lastPlays": {
    "custom_level_6B6C6FD5204104B70D56E7E43CEEDABD5319D187": "2024-02-06T07:45:42.521672+09:00",
  },
  "version": "2.1.0"
}
""");

      CollectionAssert.AreEquivalent(new List<LastPlayRecord> {
        new(DateTime.Parse("2024-02-06T07:45:42.521672+09:00"), "custom_level_6B6C6FD5204104B70D56E7E43CEEDABD5319D187")
      }, records.LatestRecords.ToList());
      Assert.AreEqual("2.1.0", records.Version);
      Assert.IsTrue(isMigrated);
    }
  }
}
