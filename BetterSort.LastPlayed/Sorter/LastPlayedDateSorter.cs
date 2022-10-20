namespace BetterSort.LastPlayed.Sorter {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using IPALogger = IPA.Logging.Logger;
  using BetterSort.Common.Interfaces;
  using BetterSort.Common.External;

  public class LastPlayedDateSorter : ISortFilter {
    /// <summary>
    /// Level id to instant.
    /// </summary>
    public Dictionary<string, DateTime> LastPlayedDates = new();

    public string Name => "Last played";

    public LastPlayedDateSorter(IClock clock, IPALogger logger) {
      _clock = clock;
      _logger = logger;
    }

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

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

    private readonly IClock _clock;
    private readonly IPALogger _logger;
    private IEnumerable<ILevelPreview>? _triggeredLevels;
    private bool _isSelected = false;

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
      _logger.Debug($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }
}
