using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public interface IScoreImporter {

    Task<List<OnlineBestRecord>?> GetPlayerBests();
  }

  public class ScoreImporterHelper(SiraLog logger, IHttpService http) {

    public async Task<string?> GetJsonWithRetry(string url) {
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
  }
}
