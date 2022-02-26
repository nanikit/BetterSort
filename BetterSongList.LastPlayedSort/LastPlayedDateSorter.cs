namespace BetterSongList.LastPlayedSort {
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

    public LastPlayedDateSorter() {
      _isVisible.value = true;
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
      _legend.value = GetLegendByLogScale(ordered, DateTime.Now, LastPlayedDates);
      return Task.CompletedTask;
    }

    private readonly ObservableVariable<bool> _isVisible = new();
    private readonly ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> _resultLevels = new();
    private readonly ObservableVariable<IEnumerable<(string, int)>> _legend = new();
    private bool _isSelected = false;

    internal static List<(string, int)> GetLegendByLogScale(IList<IPreviewBeatmapLevel> levels, DateTime now, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<(string, int)>();
      var unixNow = now.ToUnixTime();
      var lastLogOfUnixDifference = 0;

      for (var i = 0; i < levels.Count; i++) {
        var level = levels[i];
        if (lastPlayedDates.TryGetValue(level.levelID, out var lastPlayedDate)) {
          var difference = unixNow - lastPlayedDate.ToUnixTime();
          var logOfDifference = (int)Math.Log(Math.Max(3, difference), Math.E);
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
      var symbols = new string[] { "s", "m", "h", "D", "M", "Y" };
      var units = new double[] { 60, 60, 24, 30.4, 12, 1 };

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
