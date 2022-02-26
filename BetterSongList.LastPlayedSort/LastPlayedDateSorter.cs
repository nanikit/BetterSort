namespace BetterSongList.LastPlayedSort {
  using BetterSongList.LastPlayedSort.External;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  public class LastPlayedDateSorter : ISortFilter, ILegendProvider {
    /// <summary>
    /// Level id to instant.
    /// </summary>
    public Dictionary<string, DateTime>? LastPlayedDates = new();

    public string Name => "Last played";
    public ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> ResultLevels => _resultLevels;
    public ObservableVariable<bool> IsVisible => _isVisible;
    public ObservableVariable<IEnumerable<(string Label, int Index)>> Legend => _legend;

    public LastPlayedDateSorter(IClock clock) {
      _isVisible.value = true;
      _clock = clock;
    }

    public Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, bool isSelected = false, CancellationToken? token = null) {
      if (!isSelected) {
        return Task.CompletedTask;
      }

      if (LastPlayedDates == null) {
        throw new InvalidOperationException($"Precondition: {nameof(LastPlayedDates)} should not be null.");
      }

      var comparer = new LastPlayedDateComparer(LastPlayedDates);
      var ordered = newLevels.OrderBy(x => x, comparer).ToList();
      _resultLevels.value = ordered;
      _legend.value = GetLegend(ordered, LastPlayedDates);
      return Task.CompletedTask;
    }

    private readonly ObservableVariable<bool> _isVisible = new();
    private readonly ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> _resultLevels = new();
    private readonly ObservableVariable<IEnumerable<(string Label, int Index)>> _legend = new();
    private readonly IClock _clock;

    internal List<(string, int)> GetLegend(IList<IPreviewBeatmapLevel> levels, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<(string, int)>();
      var now = _clock.Now.ToLocalTime();
      var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
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
        ("Future", (date) => now < date),
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
