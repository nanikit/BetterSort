using BeatLeader;
using BetterSort.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSort.Accuracy.External {

  internal class BeatleaderBest : IScoreSource {

    public string GetRecordUrl(string playerId, int page) {
      return $"https://api.beatleader.xyz/player/{playerId}/scores?page={page}&sortBy=date&order=desc";
    }

    public (List<OnlineBestRecord> Records, PagingMetadata Paging, string? Log) ToBestRecords(string json) {
      var records = new List<OnlineBestRecord>();
      var log = new StringBuilder();

      var page = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      var data = page!.Data ?? throw new Exception("Records field is missing. Can't import from beatleader.");
      foreach (var score in data) {
        string? hash = score.Leaderboard?.Song?.Hash;
        if (hash == null) {
          log.AppendLine("Cannot get song hash from beatleader. skip.");
          continue;
        }

        var difficulty = RecordDifficultyExtension.ConvertFromString(score.Leaderboard?.Difficulty?.DifficultyName);
        if (difficulty == null) {
          log.AppendLine($"Unknown beatleader difficulty. Regard it as ExpertPlus({hash})");
        }

        records.Add(new OnlineBestRecord(
          // Cannot use modifiedScore because modifiers change star rating instead of the score for them.
          // https://github.com/BeatLeader/beatleader-server/issues/44#issuecomment-2243677732
          Accuracy: score.Accuracy * (score.Modifiers?.Contains("NF") == true ? 0.5 : 1),
          SongHash: hash.ToUpperInvariant(),
          Mode: score.Leaderboard?.Difficulty?.ModeName ?? "Standard",
          Difficulty: difficulty ?? RecordDifficulty.ExpertPlus,
          Score: score.ModifiedScore
        ));
      }

      var paging = new PagingMetadata(page.Metadata!.Page, page.Metadata.ItemsPerPage, page.Metadata.Total);
      string? logString = log.Length > 0 ? log.ToString() : null;
      return (records, paging, logString);
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

    [JsonProperty("modifiers")]
    public string? Modifiers { get; set; }
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
