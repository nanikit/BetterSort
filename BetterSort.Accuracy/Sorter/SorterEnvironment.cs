using BetterSongList;
using BetterSongList.UI;
using BetterSort.Accuracy.External;
using HarmonyLib;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Sorter {

  public class SorterEnvironment {
    private readonly UIAwareSorter _adaptor;

    private readonly IBsInterop _bsInterop;

    private readonly UnifiedImporter _importer;

    private readonly SiraLog _logger;

    private readonly IAccuracyRepository _repository;

    private readonly AccuracySorter _sorter;

    public SorterEnvironment(SiraLog logger, IAccuracyRepository repository, IBsInterop bsInterop,
      UIAwareSorter adaptor, UnifiedImporter importer, AccuracySorter sorter
    ) {
      _logger = logger;
      _repository = repository;
      _bsInterop = bsInterop;
      _adaptor = adaptor;
      _importer = importer;
      _sorter = sorter;

      _ = Initialize();
    }

    public async Task Initialize() {
      try {
        _bsInterop.OnSongPlayed += RecordHistoryWithGuard;
        _logger.Info($"Enter {nameof(SorterEnvironment)}.{nameof(Initialize)}.");

        if (!Plugin.IsTest) {
          await _importer.CollectOrImport().ConfigureAwait(false);

          bool isRegistered = SortMethods.RegisterCustomSorter(_adaptor);
          if (isRegistered) {
            var ui = AccessTools.StaticFieldRefAccess<FilterUI>(typeof(FilterUI), "persistentNuts");
            AccessTools.Method(ui.GetType(), "UpdateTransformerOptionsAndDropdowns").Invoke(ui, null);
            _logger.Info("Registered accuracy sorter.");
          }
          else {
            _logger.Info("Failed to register accuracy  sorter. Check AllowPluginSortsAndFilters config in BetterSongList.");
          }
        }
        else {
          _logger.Debug("Skip accuracy sorter registration.");
        }
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
