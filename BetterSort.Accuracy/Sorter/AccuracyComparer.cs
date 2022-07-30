namespace BetterSort.Accuracy.Sorter {
  using BetterSort.Common.Interfaces;
  using System;
  using System.Collections.Generic;

  internal class AccuracyComparer : IComparer<ILevelPreview>, IComparer<string> {
    public AccuracyComparer(IReadOnlyDictionary<string, double> accuracies) {
      _accuracies = accuracies;
    }

    public int Compare(ILevelPreview a, ILevelPreview b) {
      return Compare(a.LevelId, b.LevelId);
    }

    public int Compare(string a, string b) {
      if (_accuracies == null) {
        return 0;
      }

      if (_accuracies.TryGetValue(a, out var lastPlayOfA)) {
        if (_accuracies.TryGetValue(b, out var lastPlayOfB)) {
          int descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (_accuracies.TryGetValue(b, out var _)) {
          return 1;
        }
        return 0;
      }
    }

    /// <summary>
    /// Level id to instant.
    /// </summary>
    private readonly IReadOnlyDictionary<string, double> _accuracies;
  }
}
