namespace BetterSongList.LastPlayedSort.Sorter {
  using BetterSongList.LastPlayedSort.Core;
  using System;
  using System.Collections.Generic;

  internal class LastPlayedDateComparer : IComparer<ILevelPreview> {
    public LastPlayedDateComparer(Dictionary<string, DateTime> lastPlayedDates) {
      _lastPlayedDates = lastPlayedDates;
    }

    public int Compare(ILevelPreview a, ILevelPreview b) {
      if (_lastPlayedDates == null) {
        return 0;
      }

      if (_lastPlayedDates.TryGetValue(a.LevelId, out DateTime lastPlayOfA)) {
        if (_lastPlayedDates.TryGetValue(b.LevelId, out DateTime lastPlayOfB)) {
          int descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (_lastPlayedDates.TryGetValue(b.LevelId, out DateTime _)) {
          return 1;
        }
        return 0;
      }
    }

    /// <summary>
    /// Level id to instant.
    /// </summary>
    private readonly Dictionary<string, DateTime> _lastPlayedDates;
  }
}
