using BetterSort.Accuracy.Sorter;
using BetterSort.Common.Interfaces;
using BetterSort.Test.Common.Mocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BetterSort.Accuracy.Test {

  public class SorterTest {

    [Fact]
    public void TestNull() {
      var result = AccuracySorter.SortInternal(null, () => null);
      Assert.Null(result.Result);
    }

    // BetterSongList can pass empty list.
    [Fact]
    public void TestEmpty() {
      var result = AccuracySorter.SortInternal(new List<ILevelPreview>(), () => null);
      Assert.Empty(result.Result!.Levels);
    }

    [Fact]
    public void TestSingle() {
      var levels = new List<ILevelPreview>() { new MockPreview("custom_level_0000000000000000000000000000000000000000") };
      var records = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>() {
        { "custom_level_0000000000000000000000000000000000000000", new() {
          { "Standard", new() { { RecordDifficulty.ExpertPlus, 0.90292 } }},
        }},
      };

      var result = AccuracySorter.SortInternal(levels, () => records).Result;

      Assert.Equal(
        new List<string> { "custom_level_0000000000000000000000000000000000000000" },
        result?.Levels.Select(x => x.LevelId)!
      );
      Assert.Equal(new List<(string Label, int Index)>() { ("90.29", 0) }, result?.Legend!);
    }

    [Fact]
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

      var result = AccuracySorter.SortInternal(levels, () => records).Result;

      Assert.Equal(
        new List<string> {
          "custom_level_0000000000000000000000000000000000000000",
          "custom_level_1111111111111111111111111111111111111111",
        },
        result!.Levels.Select(x => x.LevelId)
      );
      Assert.Equal(result.Legend!, new List<(string Label, int Index)>() { ("90.29", 0), ("N/A", 1) });
    }

    [Fact]
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

      var result = AccuracySorter.SortInternal(levels, () => records).Result;

      Assert.Equal(
        new List<string> {
          "custom_level_0000000000000000000000000000000000000000",
          "custom_level_1111111111111111111111111111111111111111",
        },
        result!.Levels.Select(x => x.LevelId)
      );
      Assert.Equal(result.Legend!, new List<(string Label, int Index)>() { ("90.29", 0), ("80.00", 1) });
    }
  }
}
