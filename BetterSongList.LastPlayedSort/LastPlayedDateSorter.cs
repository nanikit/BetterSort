namespace BetterSongList.LastPlayedSort {
  using BetterSongList.LastPlayedSort.External;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  public class LastPlayedDateSorter : ISortFilter, ILegendProvider {
    /// <summary>
    /// Level id to instant.
    /// </summary>
    public Dictionary<string, DateTime>? LastPlayedDates = new();

    public string Name => "Last played";
    public ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> ResultLevels => _resultLevels;
    public ObservableVariable<bool> IsVisible => _isVisible;
    public ObservableVariable<IEnumerable<(string Label, int Index)>> Legend => _legend;

    public LastPlayedDateSorter(IClock clock) {
      _isVisible.value = true;
      _clock = clock;
    }

    public Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, bool isSelected = false, CancellationToken? token = null) {
      _triggeredLevels = newLevels;
      _isSelected = isSelected;
      Sort();
      return Task.CompletedTask;
    }

    public void RequestRefresh() {
      Sort();
    }

    private readonly ObservableVariable<bool> _isVisible = new();
    private readonly ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> _resultLevels = new();
    private readonly ObservableVariable<IEnumerable<(string Label, int Index)>> _legend = new();
    private readonly IClock _clock;
    private IEnumerable<IPreviewBeatmapLevel>? _triggeredLevels;
    bool _isSelected = false;

    private void Sort() {
      if (!_isSelected) {
        return;
      }

      if (LastPlayedDates == null) {
        throw new InvalidOperationException($"Precondition: {nameof(LastPlayedDates)} should not be null.");
      }

      var comparer = new LastPlayedDateComparer(LastPlayedDates);
      var ordered = _triggeredLevels.OrderBy(x => x, comparer).ToList();
      _resultLevels.value = ordered;
      _legend.value = DateLegendMaker.GetLegend(ordered, _clock.Now, LastPlayedDates);
    }
  }
}
