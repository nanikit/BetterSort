using BetterSort.Common.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSort.LastPlayed.External {

  public class PlayedDateRepository(SiraLog logger, IHistoryJsonRepository jsonRepository) {
    private readonly SiraLog _logger = logger;
    private readonly IHistoryJsonRepository _jsonRepository = jsonRepository;

    /// <summary>
    /// Load and migrate data if necessary.
    /// </summary>
    public StoredData? Load() {
      try {
        string? json = _jsonRepository.Load();
        if (json == null) {
          return TryImportingSongPlayHistoryData();
        }

        try {
          var (data, isMigrated) = MigrateData(json);
          _logger.Info($"{(isMigrated ? $"Loaded" : "Migrated")} {data.LatestRecords?.Count.ToString() ?? "no"} records.");
          return data;
        }
        catch (Exception exception) {
          _logger.Error(exception);
          _jsonRepository.SaveBackup(json);
          return null;
        }
      }
      catch (Exception exception) {
        _logger.Error(exception);
        return null;
      }
    }

    public void Save(IEnumerable<LastPlayRecord> playDates) {
      string json = Serialize(playDates);

      try {
        _jsonRepository.Save(json);
        _logger.Info($"Saved {playDates.Count()} records");
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    internal static (StoredData records, bool isMigrated) MigrateData(string json) {
      var data = JsonConvert.DeserializeObject<CompatibleData>(json);
      if (data?.LastPlays == null) {
        return (data ?? new(), false);
      }

      int count = data.LastPlays.Count;
      var latest = data.LatestRecords ?? new List<LastPlayRecord>();
      var records = data.LastPlays
        .Select(x => new LastPlayRecord(x.Value, x.Key, null))
        .OrderByDescending(x => x.Time)
        .ToList();
      data.LastPlays = null;
      return (new() { LatestRecords = records, Version = data.Version }, true);
    }

    internal static (List<LastPlayRecord>? Records, string? Message) ConvertSongPlayHistory(string json) {
      var history = JsonConvert.DeserializeObject<IDictionary<string, IList<Record>>>(json);
      if (history == null) {
        return (null, "Can't deserialize SongPlayData.json. Skip.");
      }

      var result = new Dictionary<string, LastPlayRecord>();
      var builder = new StringBuilder();
      string[] _sphSeparator = ["___"];
      foreach (var record in history) {
        string key = record.Key;
        string[] fields = key.Split(_sphSeparator, StringSplitOptions.None);
        if (fields.Length != 3) {
          builder.AppendLine($"Can't parse {key}. Skip.");
          continue;
        }

        string levelId = fields[0];
        RecordDifficulty? difficulty = fields[1] switch {
          "0" => RecordDifficulty.Easy,
          "1" => RecordDifficulty.Normal,
          "2" => RecordDifficulty.Hard,
          "3" => RecordDifficulty.Expert,
          "4" => RecordDifficulty.ExpertPlus,
          _ => null,
        };
        if (difficulty == null) {
          builder.AppendLine($"Can't parse difficulty {fields[1]} for {key}. Skip.");
          continue;
        }
        string type = fields[2];

        long lastPlayEpoch = record.Value.Select(x => x.Date).Max();
        var instant = DateTimeOffset.FromUnixTimeMilliseconds(lastPlayEpoch).DateTime;
        LastPlayRecord GetNewRecord(RecordDifficulty difficulty) {
          return new LastPlayRecord(instant, levelId, new PlayedMap(type, difficulty));
        }

        if (result.TryGetValue(fields[0], out var existing)) {
          result[fields[0]] = existing.Time > instant ? existing : GetNewRecord(difficulty.Value);
        }
        else {
          result[fields[0]] = GetNewRecord(difficulty.Value);
        }
      }

      var records = result.Values.OrderByDescending(x => x.Time).ToList();
      return (records, builder.Length == 0 ? null : builder.ToString());
    }

    internal static string Serialize(IEnumerable<LastPlayRecord> playDates) {
      return JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(PlayedDateRepository).Assembly.GetName().Version}",
        LatestRecords = playDates.OrderByDescending(x => x.Time).ToList(),
      }, Formatting.Indented);
    }

    private StoredData? TryImportingSongPlayHistoryData() {
      string? json = _jsonRepository.LoadPlayHistory();
      if (json == null) {
        _logger.Info("Attempting to load, but there is no existing main or SongPlayHistory mod play history.");
        return null;
      }

      var (records, message) = ConvertSongPlayHistory(json);
      if (message != null) {
        _logger.Warn(message);
      }
      if (records == null) {
        return null;
      }

      _logger.Debug($"Imported SongPlayHistory data, total count: {records.Count}");
      return new StoredData() { LatestRecords = records };
    }

    private class Record {
      public long Date = 0;
    }
  }
}
