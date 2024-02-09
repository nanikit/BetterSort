using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSort.Common.Compatibility {

  public class FilterSortAdaptor : ISorterCustom, ISorterWithLegend, ITransformerPlugin {
    private readonly SiraLog _logger;

    private readonly ISortFilter _sorter;

    private bool _isVisible = true;

    private TaskCompletionSource<ISortFilterResult?> _result = new();

    public FilterSortAdaptor(ISortFilter sorter, SiraLog logger) {
      _logger = logger;
      _sorter = sorter;
      _sorter.OnResultChanged += SaveResult;
    }

    public bool isReady => true;

    public string name => _sorter.Name;

    public bool visible => _isVisible;

    public Task Prepare(CancellationToken cancelToken) {
      _logger.Trace("Prepare() is called.");
      return Task.CompletedTask;
    }

    public void ContextSwitch(SelectLevelCategoryViewController.LevelCategory levelCategory, IAnnotatedBeatmapLevelCollection? playlist) {
      // It doesn't need this information.
    }

    public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels, bool ascending) {
      _logger.Trace($"DoSort({levels.Count()}, {ascending}) is called.");
      _result = new();
      _sorter.NotifyChange(levels.Select(level => new LevelPreview(level)), true);
      _logger.Trace($"Called NotifyChange");

      if (!_result.Task.IsCompleted) {
        _logger.Error($"Cannot get sort result. Current implementation doesn't support asynchronocity.");
        return;
      }

      var newLevels = _result.Task.Result?.Levels;
      if (newLevels != null) {
        _logger.Trace($"DoSort() newLevels[0]: {(newLevels?.Count() > 0 ? newLevels.First().SongName : "_empty")}");
        levels = newLevels.OfType<LevelPreview>().Select(preview => preview.Preview).ToList();
      }
      else {
        _logger.Info("Sorter doesn't support these maps. Skip.");
      }
    }

    public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
      _logger.Trace($"BuildLegend() is called.");
      return _result.Task.Result?.Legend.Select(x => new KeyValuePair<string, int>(x.Label, x.Index)) ?? Enumerable.Empty<KeyValuePair<string, int>>();
    }

    private void SaveResult(ISortFilterResult? result) {
      _isVisible = result != null;
      _logger.Trace($"SaveResult(): _isVisible = {_isVisible}");

      if (!_result.TrySetResult(result)) {
        _logger.Warn($"SaveResult(): TrySetResult failed.");
      }
    }
  }
}
