namespace BetterSort.Common.Interfaces {

  using System.Collections.Generic;

  public interface ISortFilterResult {
    IEnumerable<(string Label, int Index)>? Legend { get; }

    /// <summary>
    /// Sort / filter result.
    /// </summary>
    IEnumerable<ILevelPreview> Levels { get; }
  }

  public record SortFilterResult(
    IEnumerable<ILevelPreview> Levels,
    IEnumerable<(string Label, int Index)>? Legend = null) : ISortFilterResult {
  }
}
