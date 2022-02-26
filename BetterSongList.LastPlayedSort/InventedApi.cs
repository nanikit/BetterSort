using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.LastPlayedSort {
  public interface ISortFilter {
    /// <summary>
    /// Sorter / filter name, appears on dropdown.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Whether show in dropdown or not
    /// </summary>
    ObservableVariable<bool> IsVisible { get; }

    /// <summary>
    /// Notify beatmap level changes.
    /// </summary>
    /// <param name="newLevels">All levels before sort or filter.</param>
    /// <param name="isSelected">Is the result levels should be changed because it is selected?</param>
    /// <returns>Processing task.</returns>
    Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, bool isSelected = false, CancellationToken? token = null);

    /// <summary>
    /// Sort / filter result.
    /// </summary>
    ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> ResultLevels { get; }
  }

  public interface ILegendProvider {
    ObservableVariable<IEnumerable<(string Label, int Index)>> Legend { get; }
  }

  // BetterSongListApi.RegisterSorter(ISortFilter sorter);
  // BetterSongListApi.RegisterFilter(ISortFilter filter);
}
