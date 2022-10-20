namespace BetterSort.Common.Interfaces {
  using System.Collections.Generic;

  public interface ISortFilterResult {
    /// <summary>
    /// Sort / filter result.
    /// </summary>
    IEnumerable<ILevelPreview> Levels { get; }

    IEnumerable<(string Label, int Index)>? Legend { get; }
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
