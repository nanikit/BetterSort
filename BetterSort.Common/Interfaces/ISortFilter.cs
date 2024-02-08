using System;
using System.Collections.Generic;

namespace BetterSort.Common.Interfaces {

  public interface ISortFilter {

    /// <summary>
    /// Occurs when the result of the sorter/filter changes. A null value indicates that the sorter/filter is not usable.
    /// </summary>
    event Action<ISortFilterResult?> OnResultChanged;

    /// <summary>
    /// Sorter/filter name which appears in dropdown.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Notifies of beatmap level changes.
    /// </summary>
    /// <remarks>
    /// This notification may occur even if the sorter/filter is not selected,
    /// but the unselect notification is not implemented.
    /// </remarks>
    /// <param name="newLevels">All levels before sort or filter. </param>
    /// <param name="isSelected">Is the result should be changed because it is selected?</param>
    void NotifyChange(IEnumerable<ILevelPreview> newLevels, bool isSelected = false);
  }
}
