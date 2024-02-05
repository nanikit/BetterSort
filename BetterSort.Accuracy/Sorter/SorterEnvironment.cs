using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using BetterSort.Common.External;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Sorter {

  public class SorterEnvironment {
    private readonly FilterSortAdaptor _adaptor;
    private readonly IBsInterop _bsInterop;
    private readonly UnifiedImporter _importer;
    private readonly SiraLog _logger;
    private readonly IAccuracyRepository _repository;
    private readonly AccuracySorter _sorter;
    private readonly ITransformerPluginHelper _pluginHelper;

    public SorterEnvironment(SiraLog logger, IAccuracyRepository repository, IBsInterop bsInterop,
      FilterSortAdaptor adaptor, UnifiedImporter importer, AccuracySorter sorter, ITransformerPluginHelper pluginHelper
    ) {
      _logger = logger;
      _repository = repository;
      _bsInterop = bsInterop;
      _adaptor = adaptor;
      _importer = importer;
      _sorter = sorter;
      _pluginHelper = pluginHelper;

      _ = Initialize();
    }

    public async Task Initialize() {
      try {
        _bsInterop.OnSongPlayed += RecordHistoryWithGuard;
        _logger.Info($"{nameof(Initialize)}.");

        await _importer.CollectOrImport().ConfigureAwait(false);
        _pluginHelper.Register(_adaptor);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task RecordHistory(PlayRecord record) {
      var data = await _repository.Load().ConfigureAwait(false);

      var records = data?.BestRecords ?? new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>();
      var difficulty = record.Difficulty;
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

    private async void RecordHistoryWithGuard(PlayRecord record) {
      try {
        await RecordHistory(record).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }
  }
}
