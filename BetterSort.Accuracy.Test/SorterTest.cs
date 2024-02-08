using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
using BetterSort.Common.Models;
using BetterSort.Common.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.Accuracy.Test {

  [TestClass]
  public class SorterTest {

    [TestMethod]
    public void TestNull() {
      var result = AccuracySorter.SortInternal(null, () => null, new List<LevelRecord>());
      Assert.IsNull(result.Result);
    }

    // BetterSongList can pass empty list.
    [TestMethod]
    public void TestEmpty() {
      var result = AccuracySorter.SortInternal(new List<ILevelPreview>(), () => null, new List<LevelRecord>());
      CollectionAssert.AreEqual(new List<ILevelPreview>(), result.Result!.Levels.ToList());
    }

    [TestMethod]
    public void TestSingle() {
      var levels = new List<ILevelPreview>() { new MockPreview("custom_level_0000000000000000000000000000000000000000") };
      var records = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>() {
        { "custom_level_0000000000000000000000000000000000000000", new() {
          { "Standard", new() { { RecordDifficulty.ExpertPlus, 0.90292 } }},
        }},
      };

      var result = AccuracySorter.SortInternal(levels, () => records, new List<LevelRecord>()).Result;

      CollectionAssert.AreEqual(
        new List<string> { "custom_level_0000000000000000000000000000000000000000" },
        result?.Levels.Select(x => x.LevelId).ToList()
      );
      CollectionAssert.AreEqual(new List<(string Label, int Index)>() { ("90.29", 0) }, result?.Legend.ToList());
    }

    [TestMethod]
    public void TestDouble() {
      var levels = new List<ILevelPreview>() {
        new MockPreview("custom_level_1111111111111111111111111111111111111111"),
        new MockPreview("custom_level_0000000000000000000000000000000000000000"),
      };
      var records = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>() {
          { "custom_level_0000000000000000000000000000000000000000", new() {
            { "Standard", new() { { RecordDifficulty.ExpertPlus, 0.90292 } }},
          }},
        };

      var result = AccuracySorter.SortInternal(levels, () => records, new List<LevelRecord>()).Result;

      CollectionAssert.AreEqual(
        new List<string> {
          "custom_level_0000000000000000000000000000000000000000",
          "custom_level_1111111111111111111111111111111111111111",
        },
        result!.Levels.Select(x => x.LevelId).ToList()
      );
      CollectionAssert.AreEqual(new List<(string Label, int Index)>() { ("90.29", 0), ("N/A", 1) }, result.Legend.ToList());
    }

    [TestMethod]
    public void TestDoubleRecords() {
      var levels = new List<ILevelPreview>() {
        new MockPreview("custom_level_1111111111111111111111111111111111111111"),
        new MockPreview("custom_level_0000000000000000000000000000000000000000"),
      };
      var records = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>() {
          { "custom_level_0000000000000000000000000000000000000000", new() {
            { "Standard", new() { { RecordDifficulty.ExpertPlus, 0.90292 } } },
          }},
          { "custom_level_1111111111111111111111111111111111111111", new() {
            { "Standard", new() {
              { RecordDifficulty.ExpertPlus, 0.80004 },
              { RecordDifficulty.Expert, 0.92004 } }
            },
          }},
        };

      var result = AccuracySorter.SortInternal(levels, () => records, new List<LevelRecord>()).Result;

      CollectionAssert.AreEqual(
        new List<string> {
          "custom_level_1111111111111111111111111111111111111111",
          "custom_level_0000000000000000000000000000000000000000",
          "custom_level_1111111111111111111111111111111111111111",
        },
        result!.Levels.Select(x => x.LevelId).ToList()
      );
      CollectionAssert.AreEqual(new List<(string Label, int Index)>() { ("92.00", 0), ("90.29", 1), ("80.00", 2) }, result.Legend.ToList());
    }
  }
}
