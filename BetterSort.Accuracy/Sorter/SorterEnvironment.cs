using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using SiraUtil.Logging;
using System;
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

      var records = data ?? [];
      var (levelId, type, difficulty, accuracy) = record;
      AccuracyRepository.AddIfBest(records, new BestRecord(levelId, type, difficulty, accuracy));

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
