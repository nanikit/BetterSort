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
      if (data != null) {
        return;
      }

      _logger.Info("Local history is not found. Import from online.");
      var records = await CollectRecordsFromOnline().ConfigureAwait(false);
      await _repository.Save(records).ConfigureAwait(false);
    }

    public async Task<SorterData> CollectRecordsFromOnline() {
      var records = await ImportRecords().ConfigureAwait(false);
      var bests = records.Select(record => new BestRecord($"custom_level_{record.SongHash?.ToUpperInvariant()}", record.Mode, record.Difficulty, record.Accuracy));
      var sorterData = AccuracyRepository.GetSorterData(bests);
      return sorterData;
    }

    private async Task<List<OnlineBestRecord>> ImportRecords() {
      var imports = _importers.Select(x => x.GetPlayerBests()).ToArray();
      try {
        await Task.WhenAll(imports).ConfigureAwait(false);
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }

      var records = imports
        .Where(x => x.Status == TaskStatus.RanToCompletion)
        .SelectMany(x => x.Result ?? []).ToList();
      return records;
    }
  }
}
