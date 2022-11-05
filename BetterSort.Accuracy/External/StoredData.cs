using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BetterSort.Accuracy.Sorter;

namespace BetterSort.Accuracy.External {
  using BestRecords = IDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>;

  public class StoredData {
    [JsonProperty("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Keys are in-game song hash, mode, difficulty in order.
    /// </summary>
    [JsonProperty("bestRecords")]
    public BestRecords BestRecords { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>();

    [JsonProperty("lastRecordAt")]
    public DateTime? LastRecordAt { get; set; }
  }
}
