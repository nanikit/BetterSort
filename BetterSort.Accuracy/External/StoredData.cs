using BetterSort.Accuracy.Sorter;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BetterSort.Accuracy.External {

  using BestRecords = IDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>;

  public class StoredData {

    /// <summary>
    /// Keys are in-game song hash, mode, difficulty in order.
    /// </summary>
    [JsonPropertyName("bestRecords")]
    public BestRecords BestRecords { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>();

    [JsonPropertyName("lastRecordAt")]
    public DateTime? LastRecordAt { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
  }
}
