using BetterSort.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.External {
  public record class CompatibleData : StoredData {
    /// <summary>
    /// This is for compatibility with old data. Use LatestRecords instead.
    /// </summary>
    [JsonProperty("lastPlays", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, DateTime>? LastPlays { get; set; }
  }

  public record class StoredData {
    [JsonProperty("latestRecords")]
    public IReadOnlyList<LastPlayRecord>? LatestRecords { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }

  public record class LastPlayRecord(
    [JsonProperty("time")] DateTime Time,
    [JsonProperty("levelId")] string LevelId,
    [JsonProperty("map")] PlayedMap? Map = null
  );
}
