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
    public ObservableVariable<IEnumerable<(string, int)>> Legend => _legend;

    public LastPlayedDateSorter(IClock clock) {
      _isVisible.value = true;
      _clock = clock;
    }

    public Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, bool isSelected = false, CancellationToken? token = null) {
      _isSelected = isSelected;
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
    private readonly ObservableVariable<IEnumerable<(string, int)>> _legend = new();
    private readonly IClock _clock;
    private bool _isSelected = false;

    internal List<(string, int)> GetLegend(IList<IPreviewBeatmapLevel> levels, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<(string, int)>();
      var unixNow = _clock.Now.ToUnixTime();
      var lastLogOfUnixDifference = -1;

      for (var i = 0; i < levels.Count; i++) {
        var level = levels[i];
        if (lastPlayedDates.TryGetValue(level.levelID, out var lastPlayedDate)) {
          var difference = unixNow - lastPlayedDate.ToUnixTime();
          double logBase = 1.5;
          double logOffset = 100000;
          var logOfDifference = (int)(Math.Log(Math.Max(1, difference) + logOffset, logBase) - Math.Log(logOffset + 1, logBase));
          if (lastLogOfUnixDifference < logOfDifference) {
            lastLogOfUnixDifference = logOfDifference;
            legend.Add((FormatRelativeTime(difference), i));
          }
        }
        else {
          legend.Add(("Unplayed", i));
          break;
        }
      }

      return legend;
    }

    internal static string FormatRelativeTime(long unixDifference) {
      if (unixDifference < 0) {
        return "Future";
      }

      var symbols = new string[] { "sec", "min", "hour", "days", "month", "year" };
      var units = new double[] { 60, 60, 24, 30.4, 12, double.MaxValue };

      var result = new List<string>();
      var value = unixDifference;

      for (var i = 0; i < symbols.Length; i++) {
        var unit = units[i];
        var remnant = value % unit;
        result.Add($"{remnant} {symbols[i]}");

        value = (long)Math.Floor(value / unit);
        if (value <= 0) {
          break;
        }
      }

      return result.Last();
    }
  }
}
