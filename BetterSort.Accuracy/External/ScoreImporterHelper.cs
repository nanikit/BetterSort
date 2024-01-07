namespace BetterSort.Accuracy.External {

  using SiraUtil.Web;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public interface IScoreImporter {

    Task<List<BestRecord>?> GetPlayerBests();
  }

  public class ScoreImporterHelper {
    private readonly IHttpService _http;
    private readonly IPALogger _logger;

    internal ScoreImporterHelper(IPALogger logger, IHttpService http) {
      _logger = logger;
      _http = http;
    }

    public async Task<string?> GetJsonWithRetry(string url) {
      for (int i = 0; i < 3; i++) {
        var response = await _http.GetAsync(url).ConfigureAwait(false);
        if (!response.Successful) {
          string? error = await response.Error().ConfigureAwait(false);
          _logger.Error($"http request was not successful: {response.Code} {error}");
          await Task.Delay((int)Math.Pow(2, i + 1)).ConfigureAwait(false);
          continue;
        }

        return await response.ReadAsStringAsync().ConfigureAwait(false);
      }

      _logger.Warn($"Request keeps failing. Stop collecting scores.");
      return null;
    }
  }
}
