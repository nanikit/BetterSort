namespace BetterSort.Common.Interfaces {
  using System.Collections.Generic;

  public interface ISortFilterResult {
    /// <summary>
    /// Sort / filter result.
    /// </summary>
    IEnumerable<ILevelPreview> Levels { get; }

    IEnumerable<(string Label, int Index)>? Legend { get; }
  }
}
