namespace BetterSort.Accuracy.External {
  using BeatLeader;
  using Newtonsoft.Json;
  using SiraUtil.Web;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public interface IScoreImporter {
    Task<List<BestRecord>?> GetPlayerBests();
  }

  public class BeatLeaderImporter : IScoreImporter {
    private readonly IPALogger _logger;
    private readonly IHttpService _http;
    private readonly ILeaderboardId _id;

    internal BeatLeaderImporter(IPALogger logger, IHttpService http, ILeaderboardId id) {
      _logger = logger;
      _http = http;
      _id = id;
    }

    public async Task<List<BestRecord>?> GetPlayerBests() {
      string? id = await _id.GetUserId().ConfigureAwait(false);
      if (id == null) {
        _logger.Info("Cannot get user ID. Abort data import.");
        return null;
      }

      var records = await GetRecords(id).ConfigureAwait(false);
      return records;
    }

    public async Task<PagedPlayerScores> GetRecord(string platformId, int page) {
      string url = $"https://api.beatleader.xyz/player/{platformId}/scores?page={page}&sortBy=date&order=desc";
      var response = await _http.GetAsync(url).ConfigureAwait(false);
      if (!response.Successful) {
        string? error = await response.Error().ConfigureAwait(false);
        throw new Exception($"http request was not successful: {url}\n{response.Code} {error}");
      }

      string json = await response.ReadAsStringAsync().ConfigureAwait(false);
      var scores = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      return scores;
    }

    private async Task<List<BestRecord>> GetRecords(string platformId) {
      var records = new List<BestRecord>();
      for (int page = 1; ; page++) {
        _logger.Debug($"Try getting beatleader page {page}...");

        var paged = await GetRecord(platformId, page).ConfigureAwait(false);
        var data = paged?.Data;
        if (paged == null || data == null) {
          _logger.Warn("Records field is missing. Can't import from beatleader.");
          break;
        }
        foreach (var score in data) {
          string? hash = score.Leaderboard?.Song?.Hash;
          if (hash == null) {
            _logger.Warn("Cannot get song hash from beatleader. skip.");
            continue;
          }
          var difficulty = DifficultyExtension.ConvertFromString(score.Leaderboard?.Difficulty?.DifficultyName);
          if (difficulty == null) {
            _logger.Warn($"Unknown beatleader difficulty. Regard it as ExpertPlus({hash})");
          }

          records.Add(new BestRecord() {
            SongHash = hash.ToUpperInvariant(),
            Mode = score.Leaderboard?.Difficulty?.ModeName ?? "Standard",
            Difficulty = difficulty ?? BeatmapDifficulty.ExpertPlus,
            Accuracy = score.Accuracy,
            Score = score.ModifiedScore,
          });
        }
        int maxPage = (int)Math.Ceiling((double)paged.Metadata!.Total / paged.Metadata.ItemsPerPage);
        if (page >= maxPage) {
          break;
        }
      }

      return records;
    }

  }

}

namespace BeatLeader {
  using Newtonsoft.Json;
  using System.Collections.Generic;

  public class PagedPlayerScores {
    [JsonProperty("metadata")]
    public PageMetadata? Metadata;

    [JsonProperty("data")]
    public List<PlayerScore>? Data;
  }

  public class PageMetadata {
    [JsonProperty("itemsPerPage")]
    public int ItemsPerPage;

    [JsonProperty("page")]
    public int Page;

    [JsonProperty("total")]
    public int Total;
  }

  public class PlayerScore {
    [JsonProperty("leaderboard")]
    public Leaderboard? Leaderboard;

    [JsonProperty("accuracy")]
    public double Accuracy;

    [JsonProperty("modifiedScore")]
    public int ModifiedScore;
  }

  public class Leaderboard {
    [JsonProperty("id")]
    public string? Id;

    [JsonProperty("song")]
    public Song? Song;

    [JsonProperty("difficulty")]
    public Difficulty? Difficulty;
  }

  public class Song {
    [JsonProperty("id")]
    public string? Id;

    [JsonProperty("hash")]
    public string? Hash;
  }

  public class Difficulty {
    [JsonProperty("difficultyName")]
    public string? DifficultyName;

    [JsonProperty("modeName")]
    public string? ModeName;
  }
}
