using BetterSort.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  using BestRecords = Dictionary<string, Dictionary<string, Dictionary<string, double>>>;

  public class AccuracySorter : ISortFilter {
    /// <summary>
    /// Keys are in-game song hash, characteristic, difficulty in order.
    /// </summary>
    public BestRecords? BestRecords { get; set; }

    public string Name => "Accuracy";

    public AccuracySorter(IPALogger logger) {
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

    private readonly IPALogger _logger;
    private IEnumerable<ILevelPreview>? _triggeredLevels;
    private bool _isSelected = false;

    private void Sort() {
      if (!_isSelected) {
        return;
      }

      if (BestRecords == null) {
        throw new InvalidOperationException($"Precondition: {nameof(BestRecords)} should not be null.");
      }

      var comparer = new LevelAccuracyComparer(BestRecords);
      var ordered = _triggeredLevels.SelectMany(comparer.Inflate).OrderBy(x => x, comparer).ToList();
      var legend = AccuracyLegendMaker.GetLegend(ordered, comparer.LevelMap);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Debug($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? "(empty)" : ordered[0].SongName)}");
    }

  }
}
