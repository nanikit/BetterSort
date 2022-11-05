using BetterSort.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.Accuracy.Sorter {
  using BestRecords = IDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>;

  internal record class LevelRecord(string Mode, RecordDifficulty Difficulty, double Accuracy);

  internal class LevelAccuracyComparer : IComparer<ILevelPreview> {
    public Dictionary<ILevelPreview, LevelRecord> LevelMap { get; set; } = new();

    public LevelAccuracyComparer(BestRecords records) {
      _records = records;
    }

    public int Compare(ILevelPreview a, ILevelPreview b) {
      if (_records == null) {
        return 0;
      }

      if (LevelMap.TryGetValue(a, out var bestA)) {
        if (LevelMap.TryGetValue(b, out var bestB)) {
          int descending = bestB.Accuracy.CompareTo(bestA.Accuracy);
          return descending;
        }
        return -1;
      }
      else {
        if (LevelMap.ContainsKey(b)) {
          return 1;
        }
        return 0;
      }
    }

    public List<ILevelPreview> Inflate(ILevelPreview preview) {
      var result = new List<ILevelPreview>();
      if (_records.TryGetValue(preview.LevelId, out var modes) && modes.Count > 0) {
        foreach (var mode in modes) {
          foreach (var difficultyScore in mode.Value) {
            var difficulty = difficultyScore.Key;
            double accuracy = difficultyScore.Value;
            var clone = preview.Clone();
            LevelMap.Add(clone, new(mode.Key, difficulty, accuracy));
            result.Add(clone);
          }
        }
      }
      else {
        result.Add(preview);
      }
      return result;
    }

    private readonly BestRecords _records;
  }

  internal class BeatmapAccuracyComparer : IComparer<string> {
    public BeatmapAccuracyComparer(BestRecords records) {
      _records = records;
    }

    /// <summary>
    /// Use with map id that will be serialized.
    /// </summary>
    public int Compare(string a, string b) {
      if (_records == null) {
        return b.CompareTo(a);
      }

      if (_records.TryGetValue(a, out var bestA)) {
        if (_records.TryGetValue(b, out var bestB)) {
          double accuracyA = bestA.SelectMany(x => x.Value.Values).Max();
          double accuracyB = bestB.SelectMany(x => x.Value.Values).Max();
          int descending = accuracyB.CompareTo(accuracyA);
          return descending == 0 ? b.CompareTo(a) : descending;
        }
        return -1;
      }
      else {
        if (_records.ContainsKey(b)) {
          return 1;
        }
        return b.CompareTo(a);
      }
    }

    private readonly BestRecords _records;
  }
}
