using BetterSongList;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  public class SorterEnvironment {
    public SorterEnvironment(
      IPALogger logger, IAccuracyRepository repository, IPlayEventSource playEventSource,
      FilterSortAdaptor adaptor, UnifiedImporter importer) {
      _logger = logger;
      _repository = repository;
      _playEventSource = playEventSource;
      _adaptor = adaptor;
      _importer = importer;

      _ = Initialize();
    }

    public async Task Initialize() {
      try {
        _playEventSource.OnSongPlayed += RecordHistory;
        _logger.Info($"Enter {nameof(SorterEnvironment)}.{nameof(Initialize)}.");

        if (!Plugin.IsTest) {
          await _importer.CollectOrImport().ConfigureAwait(false);
          SortMethods.RegisterCustomSorter(_adaptor);
          _logger.Debug("Registered accuracy sorter.");
        }
        else {
          _logger.Debug("Skip accuracy sorter registration.");
        }
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private readonly IPALogger _logger;
    private readonly IAccuracyRepository _repository;
    private readonly IPlayEventSource _playEventSource;
    private readonly FilterSortAdaptor _adaptor;
    private readonly UnifiedImporter _importer;

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
  }
}
