using BetterSongList;
using BetterSongList.HarmonyPatches;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  public class SorterEnvironment {
    public SorterEnvironment(
      IPALogger logger, IAccuracyRepository repository, IBsInterop bsInterop,
      FilterSortAdaptor adaptor, UnifiedImporter importer, AccuracySorter sorter
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
        _bsInterop.OnSongSelected += SelectDifficultyWithGuard;
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
    private readonly IBsInterop _bsInterop;
    private readonly FilterSortAdaptor _adaptor;
    private readonly UnifiedImporter _importer;
    private readonly AccuracySorter _sorter;

    private async void SelectDifficultyWithGuard(int index, IPreviewBeatmapLevel level) {
      try {
        await SelectDifficulty(index, level).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task SelectDifficulty(int index, IPreviewBeatmapLevel level) {
      if (HookLevelCollectionTableSet.sorter != _adaptor) {
        _logger.Debug($"{nameof(SelectDifficulty)}: Not selected this sort. Skip.");
        return;
      }

      if (index >= _sorter.Mapping.Count) {
        _logger.Error($"{nameof(SelectDifficulty)}: Index {index} is out of sorted item count. Quit. ({level})");
        return;
      }

      var record = _sorter.Mapping[index];
      if (record == null) {
        _logger.Debug($"{nameof(SelectDifficulty)}: Not played level. Skip. ({level}");
        return;
      }

      string mode = record.Mode;
      var characteristic = level.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).FirstOrDefault(x => x.serializedName == mode);
      if (characteristic == null) {
        _logger.Warn($"{nameof(SelectDifficulty)}: Matching characteristic is not found. Quit. ({level}");
        return;
      }

      await _bsInterop.SetModeAndDifficulty(characteristic, record.Difficulty).ConfigureAwait(false);
    }

    private async void RecordHistoryWithGuard(PlayRecord record) {
      try {
        await RecordHistory(record).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task RecordHistory(PlayRecord record) {
      var data = await _repository.Load().ConfigureAwait(false);

      var records = data?.BestRecords ?? new Dictionary<string, Dictionary<string, Dictionary<Sorter.RecordDifficulty, double>>>();
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
  }
}
