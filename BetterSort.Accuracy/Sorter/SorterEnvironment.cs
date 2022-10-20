using BetterSongList;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  using BestRecords = Dictionary<string, Dictionary<string, Dictionary<string, double>>>;

  public class SorterEnvironment {
    public SorterEnvironment(
      IPALogger logger, IAccuracyRepository repository, IPlayEventSource playEventSource,
      AccuracySorter sorter, FilterSortAdaptor adaptor, List<IScoreImporter> importers) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _sorter = sorter;
      _adaptor = adaptor;
      _importers = importers;
    }

    public async void Start(bool register) {
      var data = await _repository.Load().ConfigureAwait(false);
      if (data == null) {
        _logger.Info("Local history is not found. Import from online.");
        data = new();
      }
      else if (data.LastRecordAt == null) {
        _logger.Info("Last record date is empty. Import from online.");
      }

      if (data.LastRecordAt == null) {
        var accuracies = data.BestRecords;
        var records = await ImportRecords().ConfigureAwait(false);
        foreach (var record in records) {
          string difficulty = record.Difficulty.ToString();
          string levelHash = $"custom_level_{record.SongHash?.ToUpperInvariant()}";
          double accuracy = record.Accuracy;

          if (accuracies.TryGetValue(levelHash, out var song)) {
            if (song.TryGetValue(record.Mode, out var mode)) {
              if (mode.TryGetValue(difficulty, out double existing)) {
                if (existing < record.Accuracy) {
                  mode[difficulty] = accuracy;
                }
              }
              else {
                mode[difficulty] = accuracy;
              }
            }
            else {
              song.Add(record.Mode, new() {
                { difficulty, accuracy }
              });
            }
          }
          else {
            accuracies.Add(levelHash, new() {
              { record.Mode, new() { { difficulty, accuracy } } }
            });
          }
        }
        data.BestRecords = accuracies;
      }
      _sorter.BestRecords = data.BestRecords.ToDictionary(x => x.Key, x => x.Value);
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
    private readonly List<IScoreImporter> _importers;

    private async Task<List<BestRecord>> ImportRecords() {
      var imports = _importers.Select(x => x.GetPlayerBests()).ToArray();
      try {
        await Task.WhenAll(imports).ConfigureAwait(false);
      }
      catch {
        // Some provider may fail, skip that.
      }
      var records = imports.Where(x => x.Status == TaskStatus.RanToCompletion).SelectMany(x => x.Result).ToList();
      return records;
    }
  }
}
