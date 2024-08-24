using BetterSort.Common.Compatibility;
using BetterSort.LastPlayed.External;
using SiraUtil.Logging;
using System;
using System.Linq;
using Zenject;

namespace BetterSort.LastPlayed.Sorter {

  public class SorterEnvironment(
    SiraLog logger, PlayedDateRepository repository, IPlayEventSource playEventSource,
    LastPlayedDateSorter sorter, FilterSortAdaptor adaptor, ITransformerPluginHelper pluginHelper) : IInitializable {

    public void Initialize() {
      try {
        var data = repository.Load();
        sorter.PlayRecords = data?.LatestRecords.ToDictionary(
          data => data.LevelId,
          data => new LevelPlayData(data.Time, data.Map)
        ) ?? [];
        playEventSource.OnSongPlayed += RecordHistory;
        pluginHelper.Register(adaptor);
      }
      catch (Exception exception) {
        logger.Error(exception);
      }
    }

    private void RecordHistory(LastPlayRecord record) {
      logger.Debug($"Record play {record.LevelId}: {record.Map?.Difficulty}");
      sorter.PlayRecords[record.LevelId] = new LevelPlayData(record.Time, record.Map);
      var updatedRecords = sorter.PlayRecords.Select(x => new LastPlayRecord(x.Value.Time, x.Key, x.Value.Map));
      repository.Save(updatedRecords);
    }
  }
}
