using BetterSort.Accuracy.External;
using BetterSort.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  public class AccuracySorter : ISortFilter {
    public string Name => "Accuracy";

    public AccuracySorter(IPALogger logger, IAccuracyRepository repository) {
      _logger = logger;
      _repository = repository;
    }

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      _isSelected = isSelected;
      _logger.Debug($"NotifyChange called: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");

      if (newLevels == null) {
        return;
      }

      _triggeredLevels = newLevels;
      _ = Sort();
    }

    private readonly IPALogger _logger;
    private readonly IAccuracyRepository _repository;
    private IEnumerable<ILevelPreview>? _triggeredLevels;
    private bool _isSelected = false;

    private async Task Sort() {
      if (!_isSelected) {
        return;
      }

      var records = await _repository.Load().ConfigureAwait(false);
      if (records == null) {
        OnResultChanged(new SortFilterResult(_triggeredLevels!));
        return;
      }

      var comparer = new LevelAccuracyComparer(records.BestRecords);
      var ordered = _triggeredLevels.SelectMany(comparer.Inflate).OrderBy(x => x, comparer).ToList();
      var legend = AccuracyLegendMaker.GetLegend(ordered, comparer.LevelMap);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Debug($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? "(empty)" : ordered[0].SongName)}");
    }
  }
}
