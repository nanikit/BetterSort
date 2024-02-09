using BetterSort.Common.External;
using BetterSort.Common.Flows;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.Sorter {
  public record class LevelPlayData(DateTime Time, PlayedMap? Map);

  public class LastPlayedDateSorter(IClock clock, SiraLog logger) : ISortFilter, IDifficultySelector {
    private readonly IClock _clock = clock;
    private readonly SiraLog _logger = logger;

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    /// <summary>
    /// Level id to play data.
    /// </summary>
    public Dictionary<string, LevelPlayData> PlayRecords { get; set; } = [];

    public string Name => "Last played";

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false) {
      _logger.Debug($"NotifyChange called: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");
      if (!isSelected || newLevels == null) {
        return;
      }

      Sort(newLevels);
    }

    public PlayedMap? SelectDifficulty(int index, LevelPreview preview) {
      if (!PlayRecords.TryGetValue(preview.LevelId, out var record)) {
        _logger.Debug($"User picked a song never played while selecting difficulty: {preview.SongName}");
        return null;
      }

      if (record.Map == null) {
        _logger.Debug($"Found no difficulty information while selecting difficulty: {preview.SongName}");
        return null;
      }

      return record.Map;
    }

    private void Sort(IEnumerable<ILevelPreview>? levels) {
      if (PlayRecords == null) {
        throw new InvalidOperationException($"Precondition: {nameof(PlayRecords)} should not be null.");
      }

      var ordered = levels
        .OrderByDescending(x => PlayRecords.TryGetValue(x.LevelId, out var data) ? data.Time : new DateTime(0))
        .ToList();
      var legend = DateLegendMaker.GetLegend(ordered, _clock.Now, PlayRecords);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Info($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }
}
