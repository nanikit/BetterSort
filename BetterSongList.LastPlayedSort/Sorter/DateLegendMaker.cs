namespace BetterSongList.LastPlayedSort.Sorter {
  using System;
  using System.Collections.Generic;

  internal class DateLegendMaker {
    public static List<(string, int)> GetLegend(IList<IPreviewBeatmapLevel> levels, DateTime now, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<(string, int)>();
      var instant = now.ToLocalTime();
      var today = new DateTime(instant.Year, instant.Month, instant.Day, 0, 0, 0, DateTimeKind.Local);
      var yesterday = today.AddDays(-1);
      var thisWeek = today.AddDays(-(int)today.DayOfWeek);
      var pastWeek = thisWeek.AddDays(-7);
      var monthAgo = today.AddMonths(-1);
      var twoMonthsAgo = today.AddMonths(-2);
      var threeMonthsAgo = today.AddMonths(-3);
      var sixMonthsAgo = today.AddMonths(-6);
      var yearAgo = today.AddYears(-1);
      var twoYearsAgo = today.AddYears(-2);
      var threeYearsAgo = today.AddYears(-3);
      var sixYearsAgo = today.AddYears(-6);

      var groups = new List<(string Label, Func<DateTime, bool> Predicate)>() {
        ("Future", (date) => instant < date),
        ("Today", (date) => today < date),
        ("Yesterday", (date) => yesterday < date),
        ("This week", (date) => thisWeek < date),
        ("Past week", (date) => pastWeek < date),
        ("a month ago", (date) => monthAgo < date),
        ("2 months ago", (date) => twoMonthsAgo < date),
        ("3 months ago", (date) => threeMonthsAgo < date),
        ("6 months ago", (date) => sixMonthsAgo < date),
        ("1 year ago", (date) => yearAgo < date),
        ("2 years ago", (date) => twoYearsAgo < date),
        ("3 years ago", (date) => threeYearsAgo < date),
        ("6 years ago", (date) => sixYearsAgo < date),
      };

      var previousLabel = "";
      for (var i = 0; i < levels.Count; i++) {
        var level = levels[i];
        if (lastPlayedDates.TryGetValue(level.levelID, out var lastPlayedDate)) {
          while (groups.Count > 0 && !groups[0].Predicate(lastPlayedDate)) {
            groups.RemoveAt(0);
          }

          if (groups.Count == 0) {
            break;
          }

          if (groups[0].Label != previousLabel) {
            legend.Add((groups[0].Label, i));
            previousLabel = groups[0].Label;
          }
        }
        else {
          legend.Add(("Unplayed", i));
          break;
        }
      }

      return legend;
    }
  }
}
