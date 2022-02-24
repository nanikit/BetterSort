#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterSongList.LastPlayedSort {
  public class LastPlayedDateSorter : IBetterListPlugin {


    /// <summary>
    /// Level id to instant.
    /// </summary>
    private Dictionary<string, DateTime>? _lastPlayedDates;

    private readonly Func<Task<Dictionary<string, DateTime>>> _historyProvider;

    public string Name => "Last played";

    public ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> ResultLevels => throw new NotImplementedException();

    public LastPlayedDateSorter(Func<Task<Dictionary<string, DateTime>>> historyProvider) {
      _historyProvider = historyProvider;
    }

    public async Task Prepare(CancellationToken cancelToken) {
      _lastPlayedDates = await _historyProvider().ConfigureAwait(false);
    }

    public int Compare(IPreviewBeatmapLevel a, IPreviewBeatmapLevel b) {
      if (_lastPlayedDates == null) {
        return 0;
      }

      if (_lastPlayedDates.TryGetValue(a.levelID, out var lastPlayOfA)) {
        if (_lastPlayedDates.TryGetValue(b.levelID, out var lastPlayOfB)) {
          var descending = lastPlayOfB.CompareTo(lastPlayOfA);
          return descending;
        }
        return -1;
      }
      else {
        if (_lastPlayedDates.TryGetValue(b.levelID, out var _)) {
          return 1;
        }
        return 0;
      }
    }

    public List<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
      var legend = new List<KeyValuePair<string, int>>();

      if (_lastPlayedDates == null || levels.Length == 0) {
        return new List<KeyValuePair<string, int>>();
      }

      GetLegendByLogScale(levels, now: DateTime.Now, lastPlayedDates: _lastPlayedDates);

      return legend;
    }

    internal static List<KeyValuePair<string, int>> GetLegendByLogScale(IPreviewBeatmapLevel[] levels, DateTime now, Dictionary<string, DateTime> lastPlayedDates) {
      var legend = new List<KeyValuePair<string, int>>();
      var unixNow = now.ToUnixTime();
      var lastLogOfUnixDifference = 0;

      for (var i = 0; i < levels.Length; i++) {
        var level = levels[i];
        if (lastPlayedDates.TryGetValue(level.levelID, out var lastPlayedDate)) {
          var difference = unixNow - lastPlayedDate.ToUnixTime();
          var logOfDifference = (int)Math.Log(Math.Max(3, difference), Math.E);
          if (lastLogOfUnixDifference < logOfDifference) {
            lastLogOfUnixDifference = logOfDifference;
            legend.Add(new KeyValuePair<string, int>(FormatRelativeTime(difference), i));
          }
        }
        else {
          legend.Add(new KeyValuePair<string, int>("Unplayed", i));
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

    public Task<Action> Initialize(CancellationToken token) {
      throw new NotImplementedException();
    }

    public Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, CancellationToken token) {
      throw new NotImplementedException();
    }
    // BetterSongList.SortModels.LastPlayedDateSorter.Test()
  }

}
