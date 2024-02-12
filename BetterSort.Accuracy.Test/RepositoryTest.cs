global using SorterData = System.Collections.Generic.Dictionary<
  string,
  System.Collections.Generic.Dictionary<(string Type, BetterSort.Common.Models.RecordDifficulty), double>
>;
using BetterSort.Accuracy.External;
using BetterSort.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BetterSort.Accuracy.Test {

  [TestClass]
  public class RepositoryTest {
    private static readonly SorterData _emptyRecords = [];

    private static readonly SorterData _singleRecords = new() {
      {
        "custom_level_000",
        new() {
          { new ("Standard", RecordDifficulty.ExpertPlus), 0.90292 },
        }
      }
    };

    private static readonly SorterData _combinedRecords = new() {
      {
        "custom_level_111",
        new() {
          { new ("Standard", RecordDifficulty.ExpertPlus), 0.90292 },
          { new ("Standard", RecordDifficulty.Expert), 0.92192 },
        }
      },
    };

    private static readonly SorterData _doubleRecords = new() {
      {
        "custom_level_222",
        new() {
          { new ("Lawless", RecordDifficulty.Hard), 0.91000 },
        }
      },
      {
        "custom_level_111",
        new() {
          { new ("Standard", RecordDifficulty.ExpertPlus), 0.90292 },
          { new ("Standard", RecordDifficulty.Expert), 0.92192 },
        }
      },
    };

    private static readonly DateTime _fixedTime = DateTime.Parse("2022-03-01T00:00:00Z");

    private static readonly string _version = typeof(AccuracyRepository).Assembly.GetName().Version.ToString();

    [TestMethod]
    public void TestSaveEmpty() {
      var (json, data) = AccuracyRepository.GetPersistentData(_emptyRecords, _fixedTime);

      Assert.AreEqual($$"""
{
  "bestRecords": [],
  "lastRecordAt": "2022-03-01T09:00:00+09:00",
  "version": "{{_version}}"
}
""", json);
      Assert.AreEqual(_fixedTime, data.LastRecordAt);
      Assert.AreEqual(0, data.BestRecords.Count);
      Assert.AreEqual(_version, data.Version);
    }

    [TestMethod]
    public void TestSaveSingle() {
      var anotherTime = DateTime.Parse("2022-03-02T00:00:00Z");

      var (json, data) = AccuracyRepository.GetPersistentData(_singleRecords, anotherTime);

      Assert.AreEqual($$"""
{
  "bestRecords": [
    {
      "levelId": "custom_level_000",
      "type": "Standard",
      "difficulty": "ExpertPlus",
      "accuracy": 0.90292
    }
  ],
  "lastRecordAt": "2022-03-02T09:00:00+09:00",
  "version": "1.0.0.0"
}
""", json);
      Assert.AreEqual(anotherTime, data.LastRecordAt);
      Assert.AreEqual(1, data.BestRecords.Count);
      Assert.AreEqual(_version, data.Version);
    }

    [TestMethod]
    public void TestSaveCombined() {
      var (json, data) = AccuracyRepository.GetPersistentData(_combinedRecords, _fixedTime);

      Assert.AreEqual($$"""
{
  "bestRecords": [
    {
      "levelId": "custom_level_111",
      "type": "Standard",
      "difficulty": "Expert",
      "accuracy": 0.92192
    },
    {
      "levelId": "custom_level_111",
      "type": "Standard",
      "difficulty": "ExpertPlus",
      "accuracy": 0.90292
    }
  ],
  "lastRecordAt": "2022-03-01T09:00:00+09:00",
  "version": "1.0.0.0"
}
""", json);
      Assert.AreEqual(_fixedTime, data.LastRecordAt);
      Assert.AreEqual(2, data.BestRecords.Count);
      Assert.AreEqual(_version, data.Version);
    }

    [TestMethod]
    public void TestSaveDouble() {
      var (json, data) = AccuracyRepository.GetPersistentData(_doubleRecords, _fixedTime);

      Assert.AreEqual($$"""
{
  "bestRecords": [
    {
      "levelId": "custom_level_111",
      "type": "Standard",
      "difficulty": "Expert",
      "accuracy": 0.92192
    },
    {
      "levelId": "custom_level_222",
      "type": "Lawless",
      "difficulty": "Hard",
      "accuracy": 0.91
    },
    {
      "levelId": "custom_level_111",
      "type": "Standard",
      "difficulty": "ExpertPlus",
      "accuracy": 0.90292
    }
  ],
  "lastRecordAt": "2022-03-01T09:00:00+09:00",
  "version": "1.0.0.0"
}
""", json);
      Assert.AreEqual(_fixedTime, data.LastRecordAt);
      Assert.AreEqual(3, data.BestRecords.Count);
      Assert.AreEqual(_version, data.Version);
    }
  }
}
