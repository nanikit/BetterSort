namespace BetterSongList.LastPlayedSort.External {
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;

  internal class StoredData {
    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("lastPlays")]
    public List<(string id, DateTime date)>? LastPlays { get; set; }
  }
}
