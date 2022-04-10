namespace BetterSongList.LastPlayedSort.Sorter {
  using BetterSongList.LastPlayedSort.Core;
  using System;
  using System.Collections.Generic;

  internal class LastPlayedDateComparer : IComparer<ILevelPreview>, IComparer<string> {
    public LastPlayedDateComparer(IReadOnlyDictionary<string, DateTime> lastPlayedDates) {
      _lastPlayedDates = lastPlayedDates;
    }

    public int Compare(ILevelPreview a, ILevelPreview b) {
      return Compare(a.LevelId, b.LevelId);
    }

    public int Compare(string a, string b) {
      if (_lastPlayedDates == null) {
        return 0;
      }

      if (_lastPlayedDates.TryGetValue(a, out DateTime lastPlayOfA)) {
        if (_lastPlayedDates.TryGetValue(b, out DateTime lastPlayOfB)) {
          int descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (_lastPlayedDates.TryGetValue(b, out DateTime _)) {
          return 1;
        }
        return 0;
      }
    }

    /// <summary>
    /// Level id to instant.
    /// </summary>
    private readonly IReadOnlyDictionary<string, DateTime> _lastPlayedDates;
  }
}
