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
    public BestRecords BestRecords { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>();

    [JsonProperty("lastRecordAt")]
    public DateTime? LastRecordAt { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }
  }
}
