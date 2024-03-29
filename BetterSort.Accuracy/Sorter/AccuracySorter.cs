using BetterSort.Accuracy.External;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BetterSort.Accuracy.Sorter {

  public class AccuracySorter(SiraLog logger, IAccuracyRepository repository) : ISortFilter {
    private readonly SiraLog _logger = logger;
    private readonly IAccuracyRepository _repository = repository;
    private bool _isSelected = false;

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public string Name => "Accuracy";
    internal List<LevelRecord> Mapping { get; private set; } = [];

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false) {
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

    internal static SortResult SortInternal(IEnumerable<ILevelPreview>? levels, Func<SorterData?> getRecords, List<LevelRecord> mapping) {
      if (levels == null) {
        return new SortResult(null, $"levels is null, give it as is.");
      }

      var records = getRecords();
      if (records == null) {
        return new SortResult(new SortFilterResult(levels), $"records is null, give it as is.");
      }

      var comparer = new LevelAccuracyComparer(records);
      var ordered = levels.SelectMany(comparer.Inflate).OrderBy(x => x, comparer).ToList();

      mapping.Clear();
      foreach (var preview in ordered) {
        if (comparer.LevelMap.TryGetValue(preview, out var record)) {
          mapping.Add(record);
        }
        else {
          break;
        }
      }

      var legend = AccuracyLegendMaker.GetLevelLegend(mapping, ordered.Count);
      return new SortResult(
        new SortFilterResult(ordered, legend),
        $"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? "(empty)" : ordered[0].SongName)}"
      );
    }

    private SortFilterResult? Sort(IEnumerable<ILevelPreview>? levels) {
      var result = SortInternal(levels, () => _repository.Load().Result, Mapping);
      _logger.Info(result.Message);
      return result.Result;
    }
  }

  record SortResult(SortFilterResult? Result, string Message);
}
