using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zenject;

namespace BetterSort.Accuracy.Sorter {

  public class SorterEnvironment(SiraLog logger, IAccuracyRepository repository, IBsInterop bsInterop,
    FilterSortAdaptor adaptor, UnifiedImporter importer, ITransformerPluginHelper pluginHelper
  ) : IInitializable {
    private readonly FilterSortAdaptor _adaptor = adaptor;
    private readonly IBsInterop _bsInterop = bsInterop;
    private readonly UnifiedImporter _importer = importer;
    private readonly SiraLog _logger = logger;
    private readonly IAccuracyRepository _repository = repository;
    private readonly ITransformerPluginHelper _pluginHelper = pluginHelper;

    public async void Initialize() {
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
