using System.Text.Json.Serialization;

namespace BetterSort.Accuracy.Sorter {

  [JsonConverter(typeof(JsonStringEnumConverter<RecordDifficulty>))]
  public enum RecordDifficulty {
    Easy = 1,
    Normal = 3,
    Hard = 5,
    Expert = 7,
    ExpertPlus = 9,
  }

  public static class DifficultyExtension {

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

    public static BeatmapDifficulty? ToGameDifficulty(this RecordDifficulty difficulty) {
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
