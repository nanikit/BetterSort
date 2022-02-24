using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.LastPlayedSort {
  public interface ISortFilter {
    /// <summary>
    /// Notify beatmap levels' changes.
    /// </summary>
    /// <param name="newLevels">All levels after addition, removal.</param>
    /// <returns>Processing task.</returns>
    Task NotifyChange(IEnumerable<IPreviewBeatmapLevel> newLevels, CancellationToken token);

    /// <summary>
    /// Sort / filter result.
    /// </summary>
    ObservableVariable<IEnumerable<IPreviewBeatmapLevel>> ResultLevels { get; }
  }

  public interface IBetterListPlugin : ISortFilter {
    /// <summary>
    /// Sorter / filter name, appears on dropdown.
    /// </summary>
    string Name { get; }

    /// <returns>A disposer.</returns>
    Task<Action> Initialize(CancellationToken token);
  }

  public interface ILegendProvider {
    ObservableVariable<IEnumerable<(string, int)>> Legend { get; }
  }

  // static void BetterSongList.RegisterSorter(IBetterListPlugin sorter);
  // static void BetterSongList.RegisterFilter(IBetterListPlugin filter);

  // for now don't care alert message, etc. I don't need that.
}
