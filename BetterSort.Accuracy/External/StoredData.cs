namespace BetterSort.Accuracy.External {
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;

  public class StoredData {
    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("bestAccuracies")]
    public IDictionary<string, double>? BestAccuracies { get; set; }


    [JsonProperty("lastRecordAt")]
    public DateTime LastRecordAt { get; set; }

    [JsonProperty("lastScoresaberSync")]
    public DateTime LastScoresaberSync { get; set; }

    [JsonProperty("lastBeatLeaderSync")]
    public DateTime LastBeatLeaderSync { get; set; }
  }
}
