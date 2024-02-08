using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetterSort.Common.Models {

  [JsonConverter(typeof(StringEnumConverter))]
  public enum RecordDifficulty {
    Easy = 1,
    Normal = 3,
    Hard = 5,
    Expert = 7,
    ExpertPlus = 9,
  }

  public record PlayedMap(
    [JsonProperty("type")] string Type,
    [JsonProperty("difficulty")] RecordDifficulty Difficulty
  );

  public static class RecordDifficultyExtension {

    public static RecordDifficulty? ConvertFromString(string? beatleaderDifficulty) {
      return beatleaderDifficulty switch {
        "Easy" => RecordDifficulty.Easy,
        "Normal" => RecordDifficulty.Normal,
        "Hard" => RecordDifficulty.Hard,
        "Expert" => RecordDifficulty.Expert,
        "ExpertPlus" => RecordDifficulty.ExpertPlus,
        _ => null,
      };
    }

    internal static BeatmapDifficulty? ToGameDifficulty(this RecordDifficulty difficulty) {
      return difficulty switch {
        RecordDifficulty.Easy => BeatmapDifficulty.Easy,
        RecordDifficulty.Normal => BeatmapDifficulty.Normal,
        RecordDifficulty.Hard => BeatmapDifficulty.Hard,
        RecordDifficulty.Expert => BeatmapDifficulty.Expert,
        RecordDifficulty.ExpertPlus => BeatmapDifficulty.ExpertPlus,
        _ => null,
      };
    }
  }
}
