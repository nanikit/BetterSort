namespace BetterSort.Common.Compatibility {
  using BetterSongList.Interfaces;
  using BetterSongList.SortModels;
  using BetterSort.Common.Interfaces;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public class FilterSortAdaptor : ISorterCustom, ISorterWithLegend, ITransformerPlugin {
    public FilterSortAdaptor(ISortFilter sorter, IPALogger logger) {
      _logger = logger;
      _sorter = sorter;
      _sorter.OnResultChanged += SaveResult;
    }

    public bool isReady => true;

    public string name => _sorter.Name;

    public bool visible => _isVisible;

    public Task Prepare(CancellationToken cancelToken) {
      _logger.Trace("FilterSortAdaptor.Prepare() is called.");
      return Task.CompletedTask;
    }

    public void ContextSwitch(SelectLevelCategoryViewController.LevelCategory levelCategory, IAnnotatedBeatmapLevelCollection? playlist) {
      // It doesn't need this information.
    }

    public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels, bool ascending) {
      _logger.Trace($"FilterSortAdaptor.DoSort({levels.Count()}, {ascending}) is called.");
      _result = new();
      _sorter.NotifyChange(levels.Select(level => new LevelPreview(level)), true);

      // Actually it sorts synchronously so sort is done already. Timeout is just for safety.
      bool isComplete = _result.Task.Wait(1000);
      if (!isComplete) {
        _logger.Error($"Timeout exceeded. Current implementation doesn't support asynchonocity.");
        return;
      }

      var newLevels = _result.Task.Result?.Levels;
      if (newLevels != null) {
        _logger.Trace($"FilterSortAdaptor.DoSort() newLevels[0]: {(newLevels?.Count() > 0 ? newLevels.First().SongName : "_empty")}");
        levels = newLevels.OfType<LevelPreview>().Select(preview => preview.Preview).ToList();
      }
      else {
        _logger.Info("Sorter doesn't support these maps. Skip.");
      }
    }

    public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
      _logger.Trace($"FilterSortAdaptor.BuildLegend() is called.");
      return _result.Task.Result?.Legend.Select(x => new KeyValuePair<string, int>(x.Label, x.Index)) ?? Enumerable.Empty<KeyValuePair<string, int>>();
    }

    private readonly IPALogger _logger;
    private readonly ISortFilter _sorter;
    private bool _isVisible = true;
    private TaskCompletionSource<ISortFilterResult?> _result = new();

    private void SaveResult(ISortFilterResult? result) {
      _isVisible = result != null;
      _logger.Trace($"FilterSortAdaptor.SaveResult(): _isVisible = {_isVisible}");

      if (!_result.TrySetResult(result)) {
        _logger.Warn($"FilterSortAdaptor.SaveResult(): TrySetResult failed.");
      }
    }
  }
}

