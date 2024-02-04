using BetterSongList;
using BetterSongList.UI;
using BetterSort.Common.Compatibility;
using BetterSort.LastPlayed.External;
using HarmonyLib;
using System;
using System.Collections.Generic;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.LastPlayed.Sorter {

  public class SorterEnvironment {
    private readonly IPALogger _logger;

    private readonly IPlayedDateRepository _repository;

    private readonly IPlayEventSource _playEventSource;

    private readonly LastPlayedDateSorter _sorter;

    private readonly FilterSortAdaptor _adaptor;

    public SorterEnvironment(IPALogger logger, IPlayedDateRepository repository, IPlayEventSource playEventSource, LastPlayedDateSorter sorter, FilterSortAdaptor adaptor) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _sorter = sorter;
      _adaptor = adaptor;
    }

    public void Start(bool register) {
      var data = _repository.Load();
      _sorter.LastPlayedDates = data?.LastPlays is Dictionary<string, DateTime> lastPlays
        ? lastPlays
        : new Dictionary<string, DateTime>();
      _playEventSource.OnSongPlayed += RecordHistory;
      if (register) {
        bool isRegistered = SortMethods.RegisterCustomSorter(_adaptor);
        if (isRegistered) {
          var ui = AccessTools.StaticFieldRefAccess<FilterUI>(typeof(FilterUI), "persistentNuts");
          AccessTools.Method(ui.GetType(), "UpdateTransformerOptionsAndDropdowns").Invoke(ui, null);
          _logger.Info("Registered accuracy sorter.");
        }
        else {
          _logger.Info("Failed to register last played sorter. Check AllowPluginSortsAndFilters config in BetterSongList.");
        }
      }
      else {
        _logger.Debug("Skip last play date sorter registration.");
      }
    }

    private void RecordHistory(string levelId, DateTime instant) {
      _logger.Debug($"Record play {levelId}: {instant}");
      _sorter.LastPlayedDates[levelId] = instant;
      _repository.Save(_sorter.LastPlayedDates);
    }
  }
}
