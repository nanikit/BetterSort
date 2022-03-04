namespace BetterSongList.LastPlayedSort.Sorter {
  using System;
  using System.Collections.Generic;

  internal class DateLegendMaker {
    public static List<(string, int)> GetLegend(IList<IPreviewBeatmapLevel> levels, DateTime now, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<(string, int)>();
      int lastLogOfUnixDifference = -1;

      double logBase = Math.Sqrt(2);
      double logOffset = 100000;

      for (int i = 0; i < levels.Count; i++) {
        IPreviewBeatmapLevel? level = levels[i];
        if (lastPlayedDates.TryGetValue(level.levelID, out DateTime lastPlayedDate)) {
          TimeSpan difference = now - lastPlayedDate;
          double seconds = difference.TotalSeconds;

          int logOfDifference = (int)(Math.Log(Math.Max(1, seconds) + logOffset, logBase) - Math.Log(logOffset + 1, logBase));
          if (lastLogOfUnixDifference < logOfDifference) {
            lastLogOfUnixDifference = logOfDifference;
            legend.Add((FormatTimeLabel(lastPlayedDate, difference), i));
          }
        }
        else {
          legend.Add(("Never", i));
          break;
        }
      }

      return legend;
    }

    private static string FormatTimeLabel(DateTime instant, TimeSpan difference) {
      if (difference.TotalDays < 1) {
        return $"{instant:HH:mm}";
      }
      else if (difference.TotalDays < 7) {
        return $"{instant:ddd}";
      }
      else if (difference.TotalDays < 365) {
        return $"{instant:MM-dd}";
      }
      else {
        return $"{instant:yyyy-MM}";
      }
    }
  }
}
