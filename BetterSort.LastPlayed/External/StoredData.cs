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

  public record LastPlayRecord(
    [property: JsonProperty("time")] DateTime Time,
    [property: JsonProperty("levelId")] string LevelId,
    [property: JsonProperty("map")] PlayedMap? Map = null
  );
}
