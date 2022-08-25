namespace BetterSort.Accuracy.Sorter {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using IPALogger = IPA.Logging.Logger;
  using BetterSort.Common.Interfaces;
  using BetterSort.Common.External;

  public class AccuracySorter : ISortFilter {
    /// <summary>
    /// Level id to accuracy.
    /// </summary>
    public Dictionary<string, double> BestAccuracies = new();

    public string Name => "Accuracy";

    public AccuracySorter(IClock clock, IPALogger logger) {
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

      if (BestAccuracies == null) {
        throw new InvalidOperationException($"Precondition: {nameof(BestAccuracies)} should not be null.");
      }

      var comparer = new AccuracyComparer(BestAccuracies);
      var ordered = _triggeredLevels.OrderBy(x => x, comparer).ToList();
      var legend = AccuracyLegendMaker.GetLegend(ordered, _clock.Now, BestAccuracies);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Debug($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }

  public class SortFilterResult : ISortFilterResult {
    public IEnumerable<ILevelPreview> Levels => _levels;
    public IEnumerable<(string Label, int Index)>? Legend => _legend;

    public SortFilterResult(IEnumerable<ILevelPreview> levels, IEnumerable<(string Label, int Index)>? legend = null) {
      _levels = levels;
      _legend = legend;
    }

    private readonly IEnumerable<ILevelPreview> _levels;
    private readonly IEnumerable<(string Label, int Index)>? _legend;
  }
}
