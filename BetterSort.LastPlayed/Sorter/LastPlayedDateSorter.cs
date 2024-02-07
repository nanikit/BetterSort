using BetterSort.Common.External;
using BetterSort.Common.Interfaces;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BetterSort.LastPlayed.Sorter {
  public record class PlayedMap(
    [JsonProperty("type")] string Type,
    [JsonProperty("difficulty")] RecordDifficulty Difficulty
  );
  public record class LevelPlayData(DateTime Time, PlayedMap? Map);

  public class LastPlayedDateSorter : ISortFilter {

    /// <summary>
    /// Level id to play data.
    /// </summary>
    public Dictionary<string, LevelPlayData> LastPlays = [];

    private readonly IClock _clock;
    private readonly SiraLog _logger;
    private bool _isSelected = false;
    private IEnumerable<ILevelPreview>? _triggeredLevels;

    public LastPlayedDateSorter(IClock clock, SiraLog logger) {
      _clock = clock;
      _logger = logger;
    }

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public string Name => "Last played";

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      _isSelected = isSelected;
      _logger.Debug($"NotifyChange called: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");

      if (newLevels == null) {
        return;
      }

      _triggeredLevels = newLevels;
      Sort();
    }

    private void Sort() {
      if (!_isSelected) {
        return;
      }

      if (LastPlays == null) {
        throw new InvalidOperationException($"Precondition: {nameof(LastPlays)} should not be null.");
      }

      var ordered = _triggeredLevels
        .OrderByDescending(x => LastPlays.TryGetValue(x.LevelId, out var data) ? data.Time : new DateTime(0))
        .ToList();
      var legend = DateLegendMaker.GetLegend(ordered, _clock.Now, LastPlays);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Info($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }
}
