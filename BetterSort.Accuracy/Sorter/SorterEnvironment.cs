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

    public async void Initialize() {
      try {
        bsInterop.OnSongPlayed += RecordHistoryWithGuard;
        logger.Info($"{nameof(Initialize)}.");

        // If we register after, registration may fail. I have no idea.
        pluginHelper.Register(adaptor);
        await importer.CollectOrImport().ConfigureAwait(false);
      }
      catch (Exception ex) {
        logger.Error(ex);
      }
    }

    private async Task RecordHistory(PlayRecord record) {
      var data = await repository.Load().ConfigureAwait(false);

      var records = data ?? [];
      var (levelId, type, difficulty, accuracy) = record;
      AccuracyRepository.AddIfBest(records, new BestRecord(levelId, type, difficulty, accuracy));

      await repository.Save(records).ConfigureAwait(false);
    }

    private async void RecordHistoryWithGuard(PlayRecord record) {
      try {
        await RecordHistory(record).ConfigureAwait(false);
      }
      catch (Exception ex) {
        logger.Error(ex);
      }
    }
  }
}
