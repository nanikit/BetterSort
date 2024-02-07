using BetterSort.Common.External;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Sorter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class RecordMigrationTest {

    [TestMethod]
    public void TestEmptyData() {
      var (data, result) = PlayedDateRepository.MigrateData(new CompatibleData() { });

      Assert.IsNull(data.LatestRecords);
      Assert.IsNull(data.Version);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestFreshData() {
      string version = "2.1.0";
      var record = new LastPlayRecord(DateTime.Now, "level_a", new PlayedMap("Standard", RecordDifficulty.ExpertPlus));

      var (data, result) = PlayedDateRepository.MigrateData(
        new CompatibleData() {
          LatestRecords = [record],
          Version = version,
        }
      );

      Assert.AreEqual(1, data.LatestRecords!.Count);
      Assert.AreEqual(record, data.LatestRecords[0]);
      Assert.AreEqual(version, data.Version);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestOldData() {
      var now = DateTime.Now;
      string level = "level_0";

      var (data, result) = PlayedDateRepository.MigrateData(
        new CompatibleData() {
          LastPlays = new Dictionary<string, DateTime> { { level, now } },
        }
      );

      Assert.AreEqual(1, data.LatestRecords!.Count);
      Assert.AreEqual(level, data.LatestRecords[0].LevelId);
      Assert.AreEqual(now, data.LatestRecords[0].Time);
      Assert.IsNull(data.LatestRecords[0].Map);
      Assert.AreEqual("Migrated 1 records.", result);
    }
  }
}
