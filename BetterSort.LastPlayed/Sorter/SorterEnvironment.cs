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
    private readonly SiraLog _logger = logger;
    private readonly PlayedDateRepository _repository = repository;
    private readonly IPlayEventSource _playEventSource = playEventSource;
    private readonly LastPlayedDateSorter _sorter = sorter;
    private readonly FilterSortAdaptor _adaptor = adaptor;
    private readonly ITransformerPluginHelper _pluginHelper = pluginHelper;

    public void Initialize() {
      try {
        var data = _repository.Load();
        _sorter.PlayRecords = data?.LatestRecords.ToDictionary(
          data => data.LevelId,
          data => new LevelPlayData(data.Time, data.Map)
        ) ?? [];
        _playEventSource.OnSongPlayed += RecordHistory;
        _pluginHelper.Register(_adaptor);
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    private void RecordHistory(LastPlayRecord record) {
      _logger.Debug($"Record play {record.LevelId}: {record.Map?.Difficulty}");
      _sorter.PlayRecords[record.LevelId] = new LevelPlayData(record.Time, record.Map);
      var updatedRecords = _sorter.PlayRecords.Select(x => new LastPlayRecord(x.Value.Time, x.Key, x.Value.Map));
      var list = updatedRecords.OrderBy(list => list.Time).ToList();
      _repository.Save(list);
    }
  }
}
