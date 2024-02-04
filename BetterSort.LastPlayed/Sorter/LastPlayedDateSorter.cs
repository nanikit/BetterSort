using BetterSort.Common.External;
using BetterSort.Common.Interfaces;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BetterSort.LastPlayed.Sorter {

  public class LastPlayedDateSorter : ISortFilter {

    /// <summary>
    /// Level id to instant.
    /// </summary>
    public Dictionary<string, DateTime> LastPlayedDates = new();

    private readonly IClock _clock;
    private readonly SiraLog _logger;
    private bool _isSelected = false;
    private IEnumerable<ILevelPreview>? _triggeredLevels;

    public LastPlayedDateSorter(IClock clock, SiraLog logger) {
      _clock = clock;
      _logger = logger;
    }

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public string Name => "Last played";

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      _isSelected = isSelected;
      _logger.Debug($"NotifyChange called: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");

      if (newLevels == null) {
        return;
      }

      _triggeredLevels = newLevels;
      Sort();
    }

    public void RequestRefresh() {
      Sort();
    }

    private void Sort() {
      if (!_isSelected) {
        return;
      }

      if (LastPlayedDates == null) {
        throw new InvalidOperationException($"Precondition: {nameof(LastPlayedDates)} should not be null.");
      }

      var comparer = new LastPlayedDateComparer(LastPlayedDates);
      var ordered = _triggeredLevels.OrderBy(x => x, comparer).ToList();
      var legend = DateLegendMaker.GetLegend(ordered, _clock.Now, LastPlayedDates);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Info($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }
}
