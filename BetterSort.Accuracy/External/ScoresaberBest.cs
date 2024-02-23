using BetterSort.Common.Models;
using Newtonsoft.Json;
using Scoresaber;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSort.Accuracy.External {

  public class ScoresaberBest : IScoreSource {

    public string GetRecordUrl(string playerId, int page) {
      return $"https://scoresaber.com/api/player/{playerId}/scores?page={page}&sort=recent";
    }

    public (List<OnlineBestRecord> Records, PagingMetadata Paging, string? Log) ToBestRecords(string json) {
      var records = new List<OnlineBestRecord>();
      var log = new StringBuilder();

      var page = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      var scores = page!.PlayerScores ?? throw new Exception("Records field is missing. Can't import from scoresaber.");
      foreach (var score in scores) {
        string? hash = score.Leaderboard?.SongHash;
        if (hash == null) {
          log.AppendLine("Cannot get song hash from scoresaber. skip.");
          continue;
        }

        int? multiplied = score.Score?.BaseScore;
        int? maxScore = score.Leaderboard?.MaxScore;
        if (multiplied > 0 && (maxScore == null || maxScore == 0)) {
          maxScore = null;
          // TODO: calculate max score
          //await _scorer.GetDifficultyBeatmap(score?.Leaderboard?.SongHash, score?.Leaderboard?.);
          //maxScore = await _scorer.CalculateMaxScore()
        }
        double? accuracyOrNull = (double?)multiplied / maxScore;
        if (accuracyOrNull is not double accuracy) {
          log.AppendLine($"Can't derive scoresaber accuracy. skip({hash}, {score.Leaderboard?.SongName})");
          continue;
        }

        var difficulty = ConvertToEnum(score.Leaderboard?.Difficulty?.Difficulty);
        if (difficulty == null) {
          log.AppendLine($"Unknown scoresaber difficulty. Regard it as ExpertPlus({hash}, {score.Leaderboard?.SongName})");
        }

        records.Add(new OnlineBestRecord(
          Accuracy: accuracy,
          SongHash: hash.ToUpperInvariant(),
          Mode: GetGameMode(score.Leaderboard?.Difficulty?.GameMode),
          Difficulty: difficulty ?? RecordDifficulty.ExpertPlus,
          Score: score.Score?.ModifiedScore ?? 0
        ));
      }

      var paging = new PagingMetadata(page.Metadata!.Page, page.Metadata.ItemsPerPage, page.Metadata.Total);
      string? logString = log.Length > 0 ? log.ToString() : null;
      return (records, paging, logString);
    }

    private static RecordDifficulty? ConvertToEnum(int? scoresaberDifficulty) {
      if (scoresaberDifficulty != null && Enum.IsDefined(typeof(RecordDifficulty), scoresaberDifficulty)) {
        return (RecordDifficulty)scoresaberDifficulty;
      }
      return null;
    }

    private static string GetGameMode(string? mode) {
      return mode?.Replace("Solo", "") ?? "Standard";
    }
  }
}

namespace Scoresaber {

  public class Leaderboard {

    // "playerScore": null,
    [JsonProperty("coverImage")]
    public string? CoverImage { get; set; }

    [JsonProperty("createdDate")]
    public string? CreatedDate { get; set; }

    [JsonProperty("dailyPlays")]
    public int DailyPlays { get; set; }

    [JsonProperty("difficulty")]
    public SongDifficulty? Difficulty { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("levelAuthorName")]
    public string? LevelAuthorName { get; set; }

    [JsonProperty("loved")]
    public bool Loved { get; set; }

    [JsonProperty("lovedDate")]
    public DateTime? LovedDate { get; set; }

    [JsonProperty("maxPP")]
    public double MaxPp { get; set; }

    [JsonProperty("maxScore")]
    public int MaxScore { get; set; }

    [JsonProperty("plays")]
    public int Plays { get; set; }

    [JsonProperty("positiveModifiers")]
    public bool PositiveModifiers { get; set; }

    [JsonProperty("qualified")]
    public bool Qualified { get; set; }

    [JsonProperty("qualifiedDate")]
    public string? QualifiedDate { get; set; }

    [JsonProperty("ranked")]
    public bool Ranked { get; set; }

    [JsonProperty("rankedDate")]
    public string? RankedDate { get; set; }

    [JsonProperty("songAuthorName")]
    public string? SongAuthorName { get; set; }

    [JsonProperty("songHash")]
    public string? SongHash { get; set; }

    [JsonProperty("songName")]
    public string? SongName { get; set; }

    [JsonProperty("songSubName")]
    public string? SongSubName { get; set; }

    [JsonProperty("stars")]
    public double Stars { get; set; }

    // "difficulties": null,
  }

  public class PagedPlayerScores {

    [JsonProperty("metadata")]
    public PageMetadata? Metadata { get; set; }

    [JsonProperty("playerScores")]
    public List<PlayerScore>? PlayerScores { get; set; }
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

    [JsonProperty("score")]
    public Score? Score { get; set; }
  }

  public class Score {

    [JsonProperty("badCuts")]
    public int BadCuts { get; set; }

    [JsonProperty("baseScore")]
    public int BaseScore { get; set; }

    [JsonProperty("fullCombo")]
    public bool FullCombo { get; set; }

    [JsonProperty("hasReplay")]
    public bool HasReplay { get; set; }

    [JsonProperty("hmd")]
    public int Hmd { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("maxCombo")]
    public int MaxCombo { get; set; }

    [JsonProperty("missedNotes")]
    public int MissedNotes { get; set; }

    [JsonProperty("modifiedScore")]
    public int ModifiedScore { get; set; }

    [JsonProperty("modifiers")]
    public string? Modifiers { get; set; }

    [JsonProperty("multiplier")]
    public double Multiplier { get; set; }

    [JsonProperty("pp")]
    public double Pp { get; set; }

    [JsonProperty("rank")]
    public int Rank { get; set; }

    [JsonProperty("timeSet")]
    public DateTime TimeSet { get; set; }

    [JsonProperty("weight")]
    public double Weight { get; set; }
  }

  public class SongDifficulty {

    [JsonProperty("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("difficultyRaw")]
    public string? DifficultyRaw { get; set; }

    [JsonProperty("gameMode")]
    public string? GameMode { get; set; }

    [JsonProperty("leaderboardId")]
    public int LeaderboardId { get; set; }
  }
}
