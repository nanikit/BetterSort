using BetterSort.Common.Compatibility;
using BetterSort.LastPlayed.External;
using SiraUtil.Logging;
using System;
using System.Linq;
using Zenject;

namespace BetterSort.LastPlayed.Sorter {

  public class SorterEnvironment : IInitializable {
    private readonly SiraLog _logger;
    private readonly PlayedDateRepository _repository;
    private readonly IPlayEventSource _playEventSource;
    private readonly LastPlayedDateSorter _sorter;
    private readonly FilterSortAdaptor _adaptor;
    private readonly ITransformerPluginHelper _pluginHelper;

    public SorterEnvironment(
      SiraLog logger, PlayedDateRepository repository, IPlayEventSource playEventSource,
      LastPlayedDateSorter sorter, FilterSortAdaptor adaptor, ITransformerPluginHelper pluginHelper) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _sorter = sorter;
      _adaptor = adaptor;
      _pluginHelper = pluginHelper;
    }

    public void Initialize() {
      try {
        var data = _repository.Load();
        _sorter.LastPlays = data?.LatestRecords.ToDictionary(
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
      _sorter.LastPlays[record.LevelId] = new LevelPlayData(record.Time, record.Map);
      var updatedRecords = _sorter.LastPlays.Select(x => new LastPlayRecord(x.Value.Time, x.Key, x.Value.Map));
      var list = updatedRecords.OrderBy(list => list.Time).ToList();
      _repository.Save(list);
    }
  }
}
