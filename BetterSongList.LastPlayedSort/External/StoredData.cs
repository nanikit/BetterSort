namespace BetterSongList.LastPlayedSort.External {
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;

  public class StoredData {
    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("lastPlays")]
    public Dictionary<string, DateTime>? LastPlays { get; set; }
  }
}
