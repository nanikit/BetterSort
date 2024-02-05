using BetterSort.Common.External;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public class UnifiedImporter {
    private readonly List<IScoreImporter> _importers;
    private readonly SiraLog _logger;
    private readonly IAccuracyRepository _repository;

    internal UnifiedImporter(SiraLog logger, List<IScoreImporter> importers, IAccuracyRepository repository) {
      _logger = logger;
      _importers = importers;
      _repository = repository;
    }

    public async Task CollectOrImport() {
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

    public async Task<BestRecords> CollectRecordsFromOnline() {
      var accuracies = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>();
      var records = await ImportRecords().ConfigureAwait(false);
      foreach (var record in records) {
        var difficulty = record.Difficulty;
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

    private async Task<List<BestRecord>> ImportRecords() {
      var imports = _importers.Select(x => x.GetPlayerBests()).ToArray();
      try {
        await Task.WhenAll(imports).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }

      var records = imports
        .Where(x => x.Status == TaskStatus.RanToCompletion)
        .SelectMany(x => x.Result ?? new()).ToList();
      return records;
    }
  }
}
