namespace BetterSort.Accuracy.Sorter {
  using BetterSongList;
  using BetterSort.Accuracy.External;
  using BetterSort.Common.Compatibility;
  using System.Collections.Generic;
  using IPALogger = IPA.Logging.Logger;

  public class SorterEnvironment {
    public SorterEnvironment(IPALogger logger, IAccuracyRepository repository, IPlayEventSource playEventSource, AccuracySorter sorter, FilterSortAdaptor adaptor) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _sorter = sorter;
      _adaptor = adaptor;
    }

    public async void Start(bool register) {
      var data = await _repository.Load().ConfigureAwait(false);
      _sorter.LastPlayedDates = data?.BestAccuracies is Dictionary<string, double> lastPlays
        ? lastPlays
        : new Dictionary<string, double>();
      //_playEventSource.OnSongPlayed += RecordHistory;
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
    private readonly AccuracySorter _sorter;
    private readonly FilterSortAdaptor _adaptor;
  }
}
