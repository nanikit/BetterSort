#nullable enable

namespace BetterSongList.LastPlayedSort.Sorter {
  using System;
  using System.Collections.Generic;
  using System.Threading;

  public interface ISortFilter {
    /// <summary>
    /// Sorter / filter name, appears on dropdown.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Notify beatmap level changes.
    /// </summary>
    /// <param name="newLevels">All levels before sort or filter. null means there is no change.</param>
    /// <param name="isSelected">Is the result levels should be changed because it is selected?</param>
    void NotifyChange(IEnumerable<IPreviewBeatmapLevel>? newLevels, bool isSelected = false, CancellationToken? token = null);

    /// <summary>
    /// null means sorter / filter is not usable.
    /// </summary>
    event Action<ISortFilterResult?> OnResultChanged;
  }

  public interface ISortFilterResult {
    /// <summary>
    /// Sort / filter result.
    /// </summary>
    IEnumerable<IPreviewBeatmapLevel> Levels { get; }

    IEnumerable<(string Label, int Index)>? Legend { get; }
  }

  public class SortFilterResult : ISortFilterResult {
    public IEnumerable<IPreviewBeatmapLevel> Levels => _levels;
    public IEnumerable<(string Label, int Index)>? Legend => _legend;

    public SortFilterResult(IEnumerable<IPreviewBeatmapLevel> levels, IEnumerable<(string Label, int Index)>? legend = null) {
      _levels = levels;
      _legend = legend;
    }

    private readonly IEnumerable<IPreviewBeatmapLevel> _levels;
    private readonly IEnumerable<(string Label, int Index)>? _legend;
  }
}
