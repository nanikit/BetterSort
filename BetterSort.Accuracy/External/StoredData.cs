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

  [JsonConverter(typeof(SingleLineConverter))]
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

  internal class SingleLineConverter : JsonConverter<BestRecord> {

    public override bool CanRead {
      get { return false; }
    }

    public override void WriteJson(JsonWriter writer, BestRecord? value, JsonSerializer serializer) {
      if (value == null) {
        writer.WriteNull();
        return;
      }

      writer.WriteStartObject();
      writer.WriteRaw($$"""
 "levelId": {{JsonConvert.ToString(value.LevelId)}}, "type": {{JsonConvert.ToString(value.Type)}}, "difficulty": "{{value.Difficulty}}", "accuracy": {{value.Accuracy}}
""");
      writer.WriteWhitespace(" ");
      writer.WriteEndObject();
    }

    public override BestRecord ReadJson(JsonReader reader, Type objectType, BestRecord? existingValue, bool hasExistingValue, JsonSerializer serializer) {
      throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }
  }
}
