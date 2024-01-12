using BetterSort.Accuracy.External;
using BetterSort.Common.Interfaces;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BetterSort.Accuracy.Sorter {

  public class AccuracySorter : ISortFilter {
    private readonly SiraLog _logger;
    private readonly IAccuracyRepository _repository;
    private bool _isSelected = false;

    public AccuracySorter(SiraLog logger, IAccuracyRepository repository) {
      _logger = logger;
      _repository = repository;
    }

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public string Name => "Accuracy";
    internal List<LevelRecord> Mapping { get; private set; } = new();

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      try {
        _isSelected = isSelected;
        _logger.Debug($"{nameof(AccuracySorter)}.{nameof(NotifyChange)}: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");

        if (newLevels == null || !_isSelected) {
          return;
        }

        OnResultChanged(Sort(newLevels));
      }
      catch (Exception ex) {
        _logger.Error(ex);
        OnResultChanged(new SortFilterResult(newLevels ?? new List<ILevelPreview>()));
      }
    }

    internal static SortResult SortInternal(IEnumerable<ILevelPreview>? levels, Func<BestRecords?> getRecords) {
      if (levels == null) {
        return new SortResult(null, $"levels is null, give it as is.");
      }

      var records = getRecords();
      if (records == null) {
        return new SortResult(
          new SortFilterResult(levels),
          $"records is null, give it as is."
        );
      }

      var comparer = new AccuracyComparer(records) { IsDescending = true };
      var ordered = levels.OrderBy(x => x.LevelId, comparer).ToList();
      var legend = AccuracyLegendMaker.GetLegend(ordered.Select(x => x.LevelId), comparer);

      return new SortResult(
        new SortFilterResult(ordered, legend),
        $"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? "(empty)" : ordered[0].SongName)}"
      );
    }

    private SortFilterResult? Sort(IEnumerable<ILevelPreview>? levels) {
      var result = SortInternal(levels, () => _repository.Load().Result?.BestRecords);
      _logger.Info($"{nameof(AccuracySorter)}.{nameof(Sort)}: {result.Message}");
      return result.Result;
    }
  }

  record SortResult(SortFilterResult? Result, string? Message);
}
