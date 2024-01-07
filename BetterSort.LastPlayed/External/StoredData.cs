namespace BetterSort.LastPlayed.External {

  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;

  public class StoredData {

    [JsonProperty("lastPlays")]
    public IDictionary<string, DateTime>? LastPlays { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }
}
