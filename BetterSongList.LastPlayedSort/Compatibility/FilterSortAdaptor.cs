namespace BetterSongList.LastPlayedSort.Compatibility {
  using BetterSongList.Interfaces;
  using BetterSongList.LastPlayedSort.Core;
  using BetterSongList.SortModels;
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
    }

    public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels, bool ascending) {
      _logger.Trace($"FilterSortAdaptor.DoSort({levels.Count()}, {ascending}) is called.");
      _result = new();
      _sorter.NotifyChange(levels.Select(level => new LevelPreview(level)), true);
      _result.Task.Wait();

      IEnumerable<ILevelPreview>? newLevels = _result.Task.Result?.Levels;
      _logger.Trace($"FilterSortAdaptor.DoSort() newLevels[0]: {(newLevels?.Count() > 0 ? newLevels.First().SongName : "_empty")}");
      if (newLevels != null) {
        levels = newLevels.OfType<LevelPreview>().Select(preview => preview.Preview).ToList();
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

  public class LevelPreview : ILevelPreview {
    public IPreviewBeatmapLevel Preview { get; private set; }

    public string LevelId { get => Preview.levelID; }
    public string SongName { get => Preview.songName; }

    public LevelPreview(IPreviewBeatmapLevel preview) {
      Preview = preview;
    }
  }
}

