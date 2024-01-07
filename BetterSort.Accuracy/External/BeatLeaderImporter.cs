namespace BetterSort.Accuracy.External {

  using BeatLeader;
  using BetterSort.Accuracy.Sorter;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public class BeatLeaderImporter : IScoreImporter {
    private readonly IPALogger _logger;
    private readonly ILeaderboardId _id;
    private readonly ScoreImporterHelper _helper;

    internal BeatLeaderImporter(IPALogger logger, ILeaderboardId id, ScoreImporterHelper helper) {
      _logger = logger;
      _id = id;
      _helper = helper;
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

    public async Task<(List<BestRecord> Records, int MaxPage)?> GetPagedRecord(string platformId, int page) {
      string url = GetUrl(platformId, page);
      string? json = await _helper.GetJsonWithRetry(url).ConfigureAwait(false);
      if (json == null) {
        return null;
      }

      return GetScores(json);
    }

    private static string GetUrl(string platformId, int page) {
      return $"https://api.beatleader.xyz/player/{platformId}/scores?page={page}&sortBy=date&order=desc";
    }

    private async Task<List<BestRecord>> GetRecords(string platformId) {
      var records = new List<BestRecord>();
      for (int page = 1; ; page++) {
        _logger.Debug($"Try getting beatleader page {page}...");

        var scores = await GetPagedRecord(platformId, page).ConfigureAwait(false);
        if (scores is not (var data, var maxPage)) {
          break;
        }

        records.AddRange(data);
        if (page >= maxPage) {
          _logger.Info("Beatleader score last page reached.");
          break;
        }
      }

      return records;
    }

    private (List<BestRecord> Records, int MaxPage)? GetScores(string json) {
      var records = new List<BestRecord>();

      var page = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      var data = page.Data;
      if (data == null) {
        _logger.Warn("Records field is missing. Can't import from beatleader.");
        return null;
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
          Difficulty = difficulty ?? RecordDifficulty.ExpertPlus,
          Accuracy = score.Accuracy,
          Score = score.ModifiedScore,
        });
      }

      int maxPage = (int)Math.Ceiling((double)page.Metadata!.Total / page.Metadata.ItemsPerPage);
      return (records, maxPage);
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
