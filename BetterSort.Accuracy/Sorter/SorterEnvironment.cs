namespace BetterSort.Accuracy.Sorter {
  using BetterSongList;
  using BetterSort.Common.Compatibility;
  using BetterSort.Accuracy.External;
  using System;
  using System.Collections.Generic;
  using IPALogger = IPA.Logging.Logger;

  public class SorterEnvironment {
    public SorterEnvironment(IPALogger logger, IAccuracyRepository repository, IPlayEventSource playEventSource, LastPlayedDateSorter sorter, FilterSortAdaptor adaptor) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _sorter = sorter;
      _adaptor = adaptor;
    }

    public void Start(bool register) {
      var data = _repository.Load();
      _sorter.LastPlayedDates = data?.BestAccuracies is Dictionary<string, DateTime> lastPlays
        ? lastPlays
        : new Dictionary<string, DateTime>();
      _playEventSource.OnSongPlayed += RecordHistory;
      if (register) {
        SortMethods.RegisterCustomSorter(_adaptor);
        _logger.Debug("Registered last play date sorter.");
      }
      else {
        _logger.Debug("Skip last play date sorter registration.");
      }
    }

    private readonly IPALogger _logger;
    private readonly IAccuracyRepository _repository;
    private readonly IPlayEventSource _playEventSource;
    private readonly LastPlayedDateSorter _sorter;
    private readonly FilterSortAdaptor _adaptor;

    private void RecordHistory(string levelId, DateTime instant) {
      _logger.Debug($"Record play {levelId}: {instant}");
      _sorter.LastPlayedDates[levelId] = instant;
    }
  }
}
