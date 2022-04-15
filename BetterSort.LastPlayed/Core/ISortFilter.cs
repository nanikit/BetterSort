namespace BetterSort.LastPlayed.Core {
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
    /// <param name="newLevels">All levels before sort or filter.</param>
    /// <param name="isSelected">Is the result levels should be changed because it is selected?</param>
    void NotifyChange(IEnumerable<ILevelPreview> newLevels, bool isSelected = false, CancellationToken? token = null);

    /// <summary>
    /// null means sorter / filter is not usable.
    /// </summary>
    event Action<ISortFilterResult?> OnResultChanged;
  }
}
