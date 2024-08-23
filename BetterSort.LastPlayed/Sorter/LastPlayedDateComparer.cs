using BetterSort.Common.Models;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.Sorter {

  /// <param name="lastPlayedDates">
  /// Level id to instant.
  /// </param>
  internal class LastPlayedDateComparer(IReadOnlyDictionary<string, DateTime> lastPlayedDates) : IComparer<ILevelPreview>, IComparer<string> {

    public int Compare(ILevelPreview a, ILevelPreview b) {
      return Compare(a.LevelId, b.LevelId);
    }

    /// <summary>
    /// Use with map id that will be serialized.
    /// </summary>
    public int Compare(string a, string b) {
      if (lastPlayedDates == null) {
        return 0;
      }

      if (lastPlayedDates.TryGetValue(a, out var lastPlayOfA)) {
        if (lastPlayedDates.TryGetValue(b, out var lastPlayOfB)) {
          int descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (lastPlayedDates.TryGetValue(b, out var _)) {
          return 1;
        }
        return 0;
      }
    }
  }
}
