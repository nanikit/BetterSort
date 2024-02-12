using BetterSort.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterSort.Accuracy.External {

  public class StoredData {

    /// <summary>
    /// Keys are in-game song hash, mode, difficulty in order.
    /// </summary>
    [JsonProperty("bestRecords")]
    public IList<BestRecord> BestRecords { get; set; } = [];

    [JsonProperty("lastRecordAt")]
    public DateTime? LastRecordAt { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }

  public record BestRecord(
    [property: JsonProperty("levelId")]
    string LevelId,
    [property: JsonProperty("type")]
    string Type,
    [property: JsonProperty("difficulty")]
    RecordDifficulty Difficulty,
    [property: JsonProperty("accuracy")]
    double Accuracy
  ) : IComparable<BestRecord> {
    public int CompareTo(BestRecord other) {
      int result = other.Accuracy.CompareTo(Accuracy);
      if (result != 0) {
        return result;
      }

      result = other.LevelId.CompareTo(LevelId);
      if (result != 0) {
        return result;
      }

      result = other.Type.CompareTo(Type);
      if (result != 0) {
        return result;
      }

      result = other.Difficulty.CompareTo(Difficulty);
      return result;
    }
  }

  public record LevelBestRecord(
    string Type,
    RecordDifficulty Difficulty,
    double Accuracy
  );
}
