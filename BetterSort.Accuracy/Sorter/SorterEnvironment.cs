using BetterSongList;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  using BestRecords = IDictionary<string, Dictionary<string, Dictionary<string, double>>>;

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

    public async Task Start(bool register) {
      _playEventSource.OnSongPlayed += RecordHistory;

      if (register) {
        await CollectOrImport().ConfigureAwait(false);
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

    private async Task CollectOrImport() {
      var data = await _repository.Load().ConfigureAwait(false);
      if (data == null) {
        _logger.Info("Local history is not found. Import from online.");
        data = new();
      }
      else if (data.LastRecordAt == null) {
        _logger.Info("Last record date is empty. Import from online.");
      }
      if (data.LastRecordAt == null) {
        var records = await CollectRecordsFromOnline().ConfigureAwait(false);
        await _repository.Save(records).ConfigureAwait(false);
      }
    }

    private async Task<BestRecords> CollectRecordsFromOnline() {
      var accuracies = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
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

      return accuracies;
    }

    private async void RecordHistory(PlayRecord record) {
      var data = await _repository.Load().ConfigureAwait(false);

      var records = data?.BestRecords ?? new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
      string difficulty = record.Difficulty.ToString();
      if (records.TryGetValue(record.LevelId, out var modes)) {
        if (modes.TryGetValue(record.Mode, out var diffs)) {
          if (diffs.TryGetValue(difficulty, out double previousAccuracy)) {
            if (record.Accuracy > previousAccuracy) {
              diffs[difficulty] = record.Accuracy;
            }
          }
          else {
            diffs[difficulty] = record.Accuracy;
          }
        }
        else {
          modes[record.Mode] = new() { { difficulty, record.Accuracy } };
        }
      }
      else {
        records[record.LevelId] = new() {
          { record.Mode, new() {
            { difficulty, record.Accuracy }
          } }
        };
      }

      await _repository.Save(records).ConfigureAwait(false);
    }

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
