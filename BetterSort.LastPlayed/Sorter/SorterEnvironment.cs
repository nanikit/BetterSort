using BetterSort.Common.Compatibility;
using BetterSort.LastPlayed.External;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using Zenject;

namespace BetterSort.LastPlayed.Sorter {

  public class SorterEnvironment : IInitializable {
    private readonly SiraLog _logger;
    private readonly ImmigrationRepository _repository;
    private readonly IPlayEventSource _playEventSource;
    private readonly LastPlayedDateSorter _sorter;
    private readonly FilterSortAdaptor _adaptor;
    private readonly ITransformerPluginHelper _pluginHelper;

    public SorterEnvironment(
      SiraLog logger, ImmigrationRepository repository, IPlayEventSource playEventSource,
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
        _sorter.LastPlayedDates = data?.LastPlays is Dictionary<string, DateTime> lastPlays
          ? lastPlays
          : new Dictionary<string, DateTime>();
        _playEventSource.OnSongPlayed += RecordHistory;
        _pluginHelper.Register(_adaptor);
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    private void RecordHistory(string levelId, DateTime instant) {
      _logger.Debug($"Record play {levelId}: {instant}");
      _sorter.LastPlayedDates[levelId] = instant;
      _repository.Save(_sorter.LastPlayedDates);
    }
  }
}
