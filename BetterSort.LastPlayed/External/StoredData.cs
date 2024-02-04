using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.External {

  public class StoredData {

    [JsonProperty("lastPlays")]
    public IDictionary<string, DateTime>? LastPlays { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }
}
