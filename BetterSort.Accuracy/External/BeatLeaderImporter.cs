using BeatLeader;
using BetterSort.Common.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public class BeatLeaderImporter(SiraLog logger, ILeaderboardId boardId, ScoreImporterHelper helper) : IScoreImporter {

    public async Task<List<OnlineBestRecord>?> GetPlayerBests() {
      string? id = await boardId.GetUserId().ConfigureAwait(false);
      if (id == null) {
        logger.Info("Cannot get user ID. Abort data import.");
        return null;
      }

      var records = await GetRecords(id).ConfigureAwait(false);
      return records;
    }

    public async Task<(List<OnlineBestRecord> Records, int MaxPage)?> GetPagedRecord(string platformId, int page) {
      string url = GetUrl(platformId, page);
      string? json = await helper.GetJsonWithRetry(url).ConfigureAwait(false);
      if (json == null) {
        return null;
      }

      return GetScores(json);
    }

    private static string GetUrl(string platformId, int page) {
      return $"https://api.beatleader.xyz/player/{platformId}/scores?page={page}&sortBy=date&order=desc";
    }

    private async Task<List<OnlineBestRecord>> GetRecords(string platformId) {
      var records = new List<OnlineBestRecord>();
      for (int page = 1; ; page++) {
        logger.Debug($"Try getting beatleader page {page}...");

        var scores = await GetPagedRecord(platformId, page).ConfigureAwait(false);
        if (scores is not (var data, var maxPage)) {
          break;
        }

        records.AddRange(data);
        if (page >= maxPage) {
          logger.Info("Beatleader score last page reached.");
          break;
        }
      }

      return records;
    }

    private (List<OnlineBestRecord> Records, int MaxPage)? GetScores(string json) {
      var records = new List<OnlineBestRecord>();

      var page = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      var data = page!.Data;
      if (data == null) {
        logger.Warn("Records field is missing. Can't import from beatleader.");
        return null;
      }

      foreach (var score in data) {
        string? hash = score.Leaderboard?.Song?.Hash;
        if (hash == null) {
          logger.Warn("Cannot get song hash from beatleader. skip.");
          continue;
        }

        var difficulty = RecordDifficultyExtension.ConvertFromString(score.Leaderboard?.Difficulty?.DifficultyName);
        if (difficulty == null) {
          logger.Warn($"Unknown beatleader difficulty. Regard it as ExpertPlus({hash})");
        }

        records.Add(new OnlineBestRecord(
          Accuracy: score.Accuracy,
          SongHash: hash.ToUpperInvariant(),
          Mode: score.Leaderboard?.Difficulty?.ModeName ?? "Standard",
          Difficulty: difficulty ?? RecordDifficulty.ExpertPlus,
          Score: score.ModifiedScore
        ));
      }

      int maxPage = (int)Math.Ceiling((double)page.Metadata!.Total / page.Metadata.ItemsPerPage);
      return (records, maxPage);
    }
  }
}

namespace BeatLeader {

  public class PagedPlayerScores {

    [JsonProperty("metadata")]
    public PageMetadata? Metadata { get; set; }

    [JsonProperty("data")]
    public List<PlayerScore>? Data { get; set; }
  }

  public class PageMetadata {

    [JsonProperty("itemsPerPage")]
    public int ItemsPerPage { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
  }

  public class PlayerScore {

    [JsonProperty("leaderboard")]
    public Leaderboard? Leaderboard { get; set; }

    [JsonProperty("accuracy")]
    public double Accuracy { get; set; }

    [JsonProperty("modifiedScore")]
    public int ModifiedScore { get; set; }
  }

  public class Leaderboard {

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("song")]
    public Song? Song { get; set; }

    [JsonProperty("difficulty")]
    public Difficulty? Difficulty { get; set; }
  }

  public class Song {

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("hash")]
    public string? Hash { get; set; }
  }

  public class Difficulty {

    [JsonProperty("difficultyName")]
    public string? DifficultyName { get; set; }

    [JsonProperty("modeName")]
    public string? ModeName { get; set; }
  }
}
