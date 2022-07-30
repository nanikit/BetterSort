namespace BetterSort.Accuracy.External {
  using BetterSort.Common.External;
  using Newtonsoft.Json;
  using SiraUtil.Web;
  using Steamworks;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public class ScoresaberRepository : IAccuracyRepository {
    internal ScoresaberRepository(IPALogger logger, IHttpService http) {
      _logger = logger;
      _http = http;
    }

    public StoredData? Load() {
      if (SteamManager.Initialized) {
        var user = SteamUser.GetSteamID();
        uint id = (uint)user.GetAccountID();
      }
      throw new NotImplementedException();
    }
    public void Save(IReadOnlyDictionary<string, double> accuracies) {
      throw new NotImplementedException();
    }

    public async Task<List<BestRecord>> GetRecords(uint steamId) {
      // OculusPlatformUserModel : IPlatformUserModel

      var records = new List<BestRecord>();
      for (int page = 1; ; page++) {
        var paged = await GetRecord(steamId, page).ConfigureAwait(false);
        records.AddRange(
          paged.PlayerScores.Select(x => new BestRecord() {
            Score = x.Score?.BaseScore ?? 0,
            SongHash = x.Leaderboard?.SongHash,
            Difficulty = x.Leaderboard?.Difficulty?.Difficulty ?? 0,
            Accuracy = x.Score?.BaseScore / x.Leaderboard?.MaxScore,
          }));
        if (page >= paged.Metadata!.Total) {
          break;
        }
      }

      return records;
    }

    private async Task<PagedPlayerScores> GetRecord(uint steamId, int page) {
      var response = await _http.GetAsync($"https://scoresaber.com/api/player/{steamId}/scores?page={page}&sort=recent").ConfigureAwait(false);
      if (!response.Successful) {
        string? error = await response.Error().ConfigureAwait(false);
        throw new Exception($"http request was not successful: {response.Code} {error}");
      }

      string json = await response.ReadAsStringAsync().ConfigureAwait(false);
      var scores = JsonConvert.DeserializeObject<PagedPlayerScores>(json);
      return scores;
    }

    private readonly IPALogger _logger;
    private readonly IHttpService _http;
  }

  public class BestRecord {
    public string? SongHash { get; set; }

    public int Difficulty { get; set; }

    public int Score { get; set; }

    public double? Accuracy { get; set; }
  }

  public class PagedPlayerScores {
    [JsonProperty("playerScores")]
    public List<PlayerScore>? PlayerScores { get; set; }

    [JsonProperty("metadata")]
    public PageMetadata? Metadata { get; set; }

  }

  public class PageMetadata {
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("itemsPerPage")]
    public int ItemsPerPage { get; set; }
  }

  public class PlayerScore {
    [JsonProperty("score")]
    public Score? Score { get; set; }

    [JsonProperty("leaderboard")]
    public Leaderboard? Leaderboard { get; set; }
  }

  public class Leaderboard {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("songHash")]
    public string? SongHash { get; set; }

    [JsonProperty("songName")]
    public string? SongName { get; set; }

    [JsonProperty("songSubName")]
    public string? SongSubName { get; set; }

    [JsonProperty("songAuthorName")]
    public string? SongAuthorName { get; set; }

    [JsonProperty("levelAuthorName")]
    public string? LevelAuthorName { get; set; }

    [JsonProperty("difficulty")]
    public SongDifficulty? Difficulty { get; set; }

    [JsonProperty("maxScore")]
    public int MaxScore { get; set; }

    [JsonProperty("createdDate")]
    public string? CreatedDate { get; set; }
    [JsonProperty("rankedDate")]
    public string? RankedDate { get; set; }
    [JsonProperty("qualifiedDate")]
    public string? QualifiedDate { get; set; }

    [JsonProperty("lovedDate")]
    public DateTime? LovedDate { get; set; }

    [JsonProperty("ranked")]
    public bool Ranked { get; set; }

    [JsonProperty("qualified")]
    public bool Qualified { get; set; }

    [JsonProperty("loved")]
    public bool Loved { get; set; }

    [JsonProperty("maxPP")]
    public double MaxPp { get; set; }

    [JsonProperty("stars")]
    public double Stars { get; set; }

    [JsonProperty("plays")]
    public int Plays { get; set; }

    [JsonProperty("dailyPlays")]
    public int DailyPlays { get; set; }

    [JsonProperty("positiveModifiers")]
    public bool PositiveModifiers { get; set; }

    // "playerScore": null,
    [JsonProperty("coverImage")]
    public string? CoverImage { get; set; }

    // "difficulties": null,
  }

  public class SongDifficulty {
    [JsonProperty("leaderboardId")]
    public int LeaderboardId { get; set; }

    [JsonProperty("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("gameMode")]
    public string? GameMode { get; set; }

    [JsonProperty("difficultyRaw")]
    public string? DifficultyRaw { get; set; }
  }

  public class Score {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("rank")]
    public int Rank { get; set; }

    [JsonProperty("baseScore")]
    public int BaseScore { get; set; }

    [JsonProperty("modifiedScore")]
    public int ModifiedScore { get; set; }

    [JsonProperty("pp")]
    public double Pp { get; set; }

    [JsonProperty("weight")]
    public double Weight { get; set; }

    [JsonProperty("modifiers")]
    public string? Modifiers { get; set; }

    [JsonProperty("multiplier")]
    public double Multiplier { get; set; }

    [JsonProperty("badCuts")]
    public int BadCuts { get; set; }

    [JsonProperty("missedNotes")]
    public int MissedNotes { get; set; }

    [JsonProperty("maxCombo")]
    public int MaxCombo { get; set; }

    [JsonProperty("fullCombo")]
    public bool FullCombo { get; set; }

    [JsonProperty("hmd")]
    public int Hmd { get; set; }

    [JsonProperty("timeSet")]
    public DateTime TimeSet { get; set; }

    [JsonProperty("hasReplay")]
    public bool HasReplay { get; set; }
  }
}
