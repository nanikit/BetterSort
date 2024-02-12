using BetterSort.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.External {
  public record CompatibleData : StoredData {
    /// <summary>
    /// This is for compatibility with old data. Use LatestRecords instead.
    /// </summary>
    [JsonProperty("lastPlays", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, DateTime>? LastPlays { get; set; }
  }

  public record StoredData {
    [JsonProperty("latestRecords")]
    public IReadOnlyList<LastPlayRecord>? LatestRecords { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }

  [JsonConverter(typeof(SingleLineConverter))]
  public record LastPlayRecord(
    [property: JsonProperty("time")] DateTime Time,
    [property: JsonProperty("levelId")] string LevelId,
    [property: JsonProperty("map")] PlayedMap? Map = null
  );

  internal class SingleLineConverter : JsonConverter<LastPlayRecord> {

    public override bool CanRead {
      get { return false; }
    }

    public override void WriteJson(JsonWriter writer, LastPlayRecord? value, JsonSerializer serializer) {
      if (value == null) {
        writer.WriteNull();
        return;
      }

      writer.WriteStartObject();
      writer.WriteRaw($$"""
 "time": {{JsonConvert.ToString(value.Time)}}, "levelId": {{JsonConvert.ToString(value.LevelId)}}
""");
      if (value.Map is var (type, difficulty)) {
        writer.WriteRaw($$"""
, "map": { "type": {{JsonConvert.ToString(type)}}, "difficulty": "{{difficulty}}" }
""");
      }
      writer.WriteWhitespace(" ");
      writer.WriteEndObject();
    }

    public override LastPlayRecord ReadJson(JsonReader reader, Type objectType, LastPlayRecord? existingValue, bool hasExistingValue, JsonSerializer serializer) {
      throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }
  }
}
