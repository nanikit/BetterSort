/// <summary>
/// Keys are in-game song hash, mode, difficulty in order.
/// </summary>
global using BestRecords = System.Collections.Generic.IDictionary<
  string, System.Collections.Generic.Dictionary<
    string, System.Collections.Generic.Dictionary<BetterSort.Accuracy.Sorter.RecordDifficulty, double>
  >
>;

using BetterSort.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.Accuracy.Sorter {
  internal record class LevelRecord(string Mode, RecordDifficulty Difficulty, double Accuracy);

  internal class LevelAccuracyComparer : IComparer<ILevelPreview> {
    private readonly BestRecords _records;

    public LevelAccuracyComparer(BestRecords records) {
      _records = records;
    }

    public Dictionary<ILevelPreview, LevelRecord> LevelMap { get; set; } = new();

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
  }

  record class BeatmapRecord(ILevelPreview Preview, (double, double)? AccuracyRange);

  /// <summary>
  /// Compares level ID.
  /// <code>
  /// Ascending sort:  ↑ Not played
  ///                  ↓ Higher max level accuracy
  /// Descending sort: ↑ Higher min level accuracy
  ///                  ↓ Not played
  /// </code>
  /// </summary>
  internal class AccuracyComparer : IComparer<string> {
    private readonly BestRecords _records;

    public AccuracyComparer(BestRecords records) {
      _records = records;
    }

    public bool IsDescending { get; set; }

    public int Compare(string a, string b) {
      int comparison = CompareAscending(a, b);
      return IsDescending ? -comparison : comparison;
    }

    public double? GetAccuracy(string levelId) {
      if (!_records.TryGetValue(levelId, out var best)) {
        return null;
      }

      double[] accuracies = best.Values.SelectMany(x => x.Values).ToArray();
      if (accuracies.Length == 0) {
        return null;
      }

      return IsDescending ? accuracies.Max() : accuracies.Min();
    }

    private int CompareAscending(string a, string b) {
      if (GetAccuracy(a) is double bestA) {
        if (GetAccuracy(b) is double bestB) {
          return bestA.CompareTo(bestB);
        }
        return 1;
      }
      else {
        if (GetAccuracy(b) is not null) {
          return -1;
        }
        return a.CompareTo(b);
      }
    }
  }
}
