using BetterSort.Accuracy.Sorter;
using Scoresaber;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public class ScoresaberImporter : IScoreImporter {
    private readonly ScoreImporterHelper _helper;
    private readonly ILeaderboardId _id;
    private readonly SiraLog _logger;

    internal ScoresaberImporter(SiraLog logger, ILeaderboardId id, ScoreImporterHelper helper) {
      _logger = logger;
      _id = id;
      _helper = helper;
    }

    public async Task<(List<BestRecord> Records, int MaxPage)?> GetPagedRecord(string platformId, int page) {
      string url = GetUrl(platformId, page);
      string? json = await _helper.GetJsonWithRetry(url).ConfigureAwait(false);
      if (json == null) {
        return null;
      }

      return GetScores(json);
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

    private static RecordDifficulty? ConvertToEnum(int? scoresaberDifficulty) {
      return scoresaberDifficulty switch {
        1 => RecordDifficulty.Easy,
        3 => RecordDifficulty.Normal,
        5 => RecordDifficulty.Hard,
        7 => RecordDifficulty.Expert,
        9 => RecordDifficulty.ExpertPlus,
        _ => null,
      };
    }

    private static string GetGameMode(string? mode) {
      return mode?.Replace("Solo", "") ?? "Standard";
    }

    private static string GetUrl(string platformId, int page) {
      return $"https://scoresaber.com/api/player/{platformId}/scores?page={page}&sort=recent";
    }

    private async Task<List<BestRecord>> GetRecords(string platformId) {
      var records = new List<BestRecord>();
      for (int page = 1; ; page++) {
        _logger.Debug($"Try getting scoresaber page {page}...");

        var scores = await GetPagedRecord(platformId, page).ConfigureAwait(false);
        if (scores is not (var data, var maxPage)) {
          break;
        }

        records.AddRange(data);
        if (page >= maxPage) {
          _logger.Info("Scoresaber score last page reached.");
          break;
        }
      }

      return records;
    }

    private (List<BestRecord> Records, int MaxPage)? GetScores(string json) {
      var records = new List<BestRecord>();

      var page = JsonSerializer.Deserialize<PagedPlayerScores>(json);
      var scores = page!.PlayerScores;
      if (scores == null) {
        _logger.Warn("Records field is missing. Can't import from scoresaber.");
        return null;
      }

      foreach (var score in scores) {
        string? hash = score.Leaderboard?.SongHash;
        if (hash == null) {
          _logger.Warn("Cannot get song hash from scoresaber. skip.");
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
          _logger.Warn($"Can't derive scoresaber accuracy. skip({hash}, {score.Leaderboard?.SongName})");
          continue;
        }
        var difficulty = ConvertToEnum(score.Leaderboard?.Difficulty?.Difficulty);
        if (difficulty == null) {
          _logger.Warn($"Unknown scoresaber difficulty. Regard it as ExpertPlus({hash}, {score.Leaderboard?.SongName})");
        }

        records.Add(new BestRecord() {
          SongHash = hash,
          Mode = GetGameMode(score.Leaderboard?.Difficulty?.GameMode),
          Difficulty = difficulty ?? RecordDifficulty.ExpertPlus,
          Accuracy = accuracy,
          Score = score.Score?.ModifiedScore ?? 0,
        });
      }

      int maxPage = (int)Math.Ceiling((double)page.Metadata!.Total / page.Metadata.ItemsPerPage);
      return (records, maxPage);
    }
  }
}

namespace Scoresaber {

  public class Leaderboard {

    // "playerScore": null,
    [JsonPropertyName("coverImage")]
    public string? CoverImage { get; set; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; set; }

    [JsonPropertyName("dailyPlays")]
    public int DailyPlays { get; set; }

    [JsonPropertyName("difficulty")]
    public SongDifficulty? Difficulty { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("levelAuthorName")]
    public string? LevelAuthorName { get; set; }

    [JsonPropertyName("loved")]
    public bool Loved { get; set; }

    [JsonPropertyName("lovedDate")]
    public DateTime? LovedDate { get; set; }

    [JsonPropertyName("maxPP")]
    public double MaxPp { get; set; }

    [JsonPropertyName("maxScore")]
    public int MaxScore { get; set; }

    [JsonPropertyName("plays")]
    public int Plays { get; set; }

    [JsonPropertyName("positiveModifiers")]
    public bool PositiveModifiers { get; set; }

    [JsonPropertyName("qualified")]
    public bool Qualified { get; set; }

    [JsonPropertyName("qualifiedDate")]
    public string? QualifiedDate { get; set; }

    [JsonPropertyName("ranked")]
    public bool Ranked { get; set; }

    [JsonPropertyName("rankedDate")]
    public string? RankedDate { get; set; }

    [JsonPropertyName("songAuthorName")]
    public string? SongAuthorName { get; set; }

    [JsonPropertyName("songHash")]
    public string? SongHash { get; set; }

    [JsonPropertyName("songName")]
    public string? SongName { get; set; }

    [JsonPropertyName("songSubName")]
    public string? SongSubName { get; set; }

    [JsonPropertyName("stars")]
    public double Stars { get; set; }

    // "difficulties": null,
  }

  public class PagedPlayerScores {

    [JsonPropertyName("metadata")]
    public PageMetadata? Metadata { get; set; }

    [JsonPropertyName("playerScores")]
    public List<PlayerScore>? PlayerScores { get; set; }
  }

  public class PageMetadata {

    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
  }

  public class PlayerScore {

    [JsonPropertyName("leaderboard")]
    public Leaderboard? Leaderboard { get; set; }

    [JsonPropertyName("score")]
    public Score? Score { get; set; }
  }

  public class Score {

    [JsonPropertyName("badCuts")]
    public int BadCuts { get; set; }

    [JsonPropertyName("baseScore")]
    public int BaseScore { get; set; }

    [JsonPropertyName("fullCombo")]
    public bool FullCombo { get; set; }

    [JsonPropertyName("hasReplay")]
    public bool HasReplay { get; set; }

    [JsonPropertyName("hmd")]
    public int Hmd { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("maxCombo")]
    public int MaxCombo { get; set; }

    [JsonPropertyName("missedNotes")]
    public int MissedNotes { get; set; }

    [JsonPropertyName("modifiedScore")]
    public int ModifiedScore { get; set; }

    [JsonPropertyName("modifiers")]
    public string? Modifiers { get; set; }

    [JsonPropertyName("multiplier")]
    public double Multiplier { get; set; }

    [JsonPropertyName("pp")]
    public double Pp { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("timeSet")]
    public DateTime TimeSet { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }
  }

  public class SongDifficulty {

    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; }

    [JsonPropertyName("difficultyRaw")]
    public string? DifficultyRaw { get; set; }

    [JsonPropertyName("gameMode")]
    public string? GameMode { get; set; }

    [JsonPropertyName("leaderboardId")]
    public int LeaderboardId { get; set; }
  }
}
