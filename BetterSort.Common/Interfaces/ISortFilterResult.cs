namespace BetterSort.Common.Interfaces {

  using System.Collections.Generic;

  public interface ISortFilterResult {
    IEnumerable<(string Label, int Index)>? Legend { get; }

    /// <summary>
    /// Sort / filter result.
    /// </summary>
    IEnumerable<ILevelPreview> Levels { get; }
  }

  public class SortFilterResult : ISortFilterResult {
    private readonly IEnumerable<(string Label, int Index)>? _legend;
    private readonly IEnumerable<ILevelPreview> _levels;

    public SortFilterResult(IEnumerable<ILevelPreview> levels, IEnumerable<(string Label, int Index)>? legend = null) {
      _levels = levels;
      _legend = legend;
    }

    public IEnumerable<(string Label, int Index)>? Legend => _legend;
    public IEnumerable<ILevelPreview> Levels => _levels;
  }
}
