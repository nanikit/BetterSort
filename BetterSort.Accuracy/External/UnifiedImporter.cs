using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public class UnifiedImporter(
    SiraLog logger, List<IScoreSource> importers, IAccuracyRepository repository,
    ILeaderboardId boardId, IHttpService http, IProgressBar progress) {
    private int _totalPages;
    private int _fetchedPages;

    public async Task CollectOrImport() {
      var data = await repository.Load().ConfigureAwait(false);
      if (data != null) {
        return;
      }

      logger.Info("Local history is not found. Import from online.");
      var records = await CollectRecordsFromOnline().ConfigureAwait(false);
      await repository.Save(records).ConfigureAwait(false);
    }

    public async Task<SorterData> CollectRecordsFromOnline() {
      var records = await ImportRecords().ConfigureAwait(false);
      var bests = records.Select(record => new BestRecord($"custom_level_{record.SongHash?.ToUpperInvariant()}", record.Mode, record.Difficulty, record.Accuracy));
      var sorterData = AccuracyRepository.GetSorterData(bests);
      return sorterData;
    }

    private async Task<string?> GetJsonWithRetry(string url) {
      for (int i = 0; i < 3; i++) {
        var response = await http.GetAsync(url).ConfigureAwait(false);
        if (!response.Successful) {
          string? error = await response.Error().ConfigureAwait(false);
          logger.Error($"http request was not successful: {response.Code} {error}");
          await Task.Delay((int)Math.Pow(2, i + 1)).ConfigureAwait(false);
          continue;
        }

        return await response.ReadAsStringAsync().ConfigureAwait(false);
      }

      logger.Warn($"Request keeps failing. Stop collecting scores.");
      return null;
    }

    private async Task<List<OnlineBestRecord>> ImportRecords() {
      string? playerId = await boardId.GetUserId().ConfigureAwait(false);
      if (playerId == null) {
        logger.Info("Cannot get user ID. Abort data import.");
        return [];
      }

      _totalPages = 0;
      var imports = importers.Select(x => GetPlayerBests(x, playerId)).ToArray();
      try {
        await Task.WhenAll(imports).ConfigureAwait(false);
      }
      catch (Exception ex) {
        logger.Error(ex);
      }

      var records = imports
        .Where(x => x.Status == TaskStatus.RanToCompletion)
        .SelectMany(x => x.Result ?? []).ToList();

      var message = new ProgressMessage("BetterSort.Accuracy", "Import finished.", "", null, TimeSpan.FromSeconds(5));
      _ = progress.SetMessage(message);
      return records;
    }

    private async Task<List<OnlineBestRecord>> GetPlayerBests(IScoreSource importer, string playerId) {
      var result = new List<OnlineBestRecord>();
      int failureCount = 0;
      var message = new ProgressMessage("BetterSort.Accuracy", "Importing records from BL and SS", "", 0, TimeSpan.MaxValue);

      for (int page = 0; ; page++) {
        if (failureCount > 3) {
          logger.Warn($"Too many failures. Stop importing {importer.GetType().Name} score.");
          break;
        }

        try {
          logger.Info($"Importing {importer.GetType().Name} score page {page}.");

          string url = importer.GetRecordUrl(playerId, page);
          string? json = await GetJsonWithRetry(url).ConfigureAwait(false);
          if (json == null) {
            failureCount++;
            continue;
          }

          var (records, paging, log) = importer.ToBestRecords(json);
          if (log != null) {
            logger.Warn(log);
          }
          result.AddRange(records);

          int maxPage = (int)Math.Ceiling((double)paging.Total / paging.ItemsPerPage);
          if (page >= maxPage) {
            logger.Info($"{importer.GetType().Name} score last page reached.");
            break;
          }

          _fetchedPages++;
          if (page == 1) {
            _totalPages += maxPage;
          }
          message = message with {
            Progress = $"{_fetchedPages} / {_totalPages}",
            Ratio = (float)_fetchedPages / _totalPages
          };
          _ = progress.SetMessage(message);
        }
        catch (Exception exception) {
          failureCount++;
          logger.Error(exception);
        }
      }

      return result;
    }
  }
}
