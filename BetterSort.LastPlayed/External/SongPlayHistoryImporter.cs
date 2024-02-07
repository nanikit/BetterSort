using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.External {

  public class SongPlayHistoryImporter(SiraLog logger, IHistoryJsonRepository jsonRepository) {
    private static readonly string[] _sphSeparator = ["___"];

    private readonly SiraLog _logger = logger;
    private readonly IHistoryJsonRepository _jsonRepository = jsonRepository;

    // TODO: preserve difficulty information
    public Dictionary<string, DateTime>? Load() {
      string? json = _jsonRepository.LoadPlayHistory();
      if (json == null) {
        _logger.Warn("SongPlayHistory file seems not exists. Skip import.");
        return null;
      }

      var result = new Dictionary<string, DateTime>();
      var history = JsonConvert.DeserializeObject<IDictionary<string, IList<Record>>>(json);
      if (history == null) {
        _logger.Warn("Can't deserialize SongPlayData.json. Skip.");
        return null;
      }

      foreach (var record in history) {
        string difficulty = record.Key;
        string[] fields = difficulty.Split(_sphSeparator, StringSplitOptions.None);
        if (fields.Length != 3) {
          _logger.Warn($"Can't parse {difficulty}. Skip.");
          continue;
        }

        long lastPlayEpoch = record.Value.Select(x => x.Date).Max();
        var instant = DateTimeOffset.FromUnixTimeMilliseconds(lastPlayEpoch).DateTime;
        if (result.TryGetValue(fields[0], out var existing)) {
          result[fields[0]] = instant < existing ? existing : instant;
        }
        else {
          result[fields[0]] = instant;
        }
      }

      return result;
    }

    private class Record {
#pragma warning disable 0649
      public long Date;
#pragma warning restore 0649
    }
  }
}
