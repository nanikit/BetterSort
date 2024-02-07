using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.External {

  public class PlayedDateRepository(SiraLog logger, IPlayedDateJsonRepository jsonRepository) {
    private readonly SiraLog _logger = logger;
    private readonly IPlayedDateJsonRepository _jsonRepository = jsonRepository;

    /// <summary>
    /// Load and migrate data if necessary.
    /// </summary>
    public StoredData? Load() {
      try {
        string? json = _jsonRepository.Load();
        if (json == null) {
          _logger.Debug($"Attempting to load, but there is no existing play history.");
          return null;
        }

        var compatibleData = JsonConvert.DeserializeObject<CompatibleData>(json);

        var (data, migrationResult) = MigrateData(compatibleData);
        if (migrationResult != null) {
          _logger.Info(migrationResult);
        }

        _logger.Info($"Loaded {data.LatestRecords?.Count.ToString() ?? "no"} records");
        return data;
      }
      catch (Exception exception) {
        _logger.Error(exception);
        return null;
      }
    }

    public void Save(IReadOnlyList<LastPlayRecord> playDates) {
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(PlayedDateRepository).Assembly.GetName().Version}",
        LatestRecords = playDates,
      }, Formatting.Indented);

      try {
        _jsonRepository.Save(json);
        _logger.Info($"Saved {playDates.Count} records");
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    internal static (StoredData, string?) MigrateData(CompatibleData? data) {
      if (data?.LastPlays == null) {
        return (data ?? new(), null);
      }

      int count = data.LastPlays.Count;
      var latest = data.LatestRecords ?? new List<LastPlayRecord>();
      var records = data.LastPlays
        .Select(x => new LastPlayRecord(x.Value, x.Key, null))
        .OrderByDescending(x => x.Time)
        .ToList();
      data.LastPlays = null;
      return (new() { LatestRecords = records }, $"Migrated {count} records.");
    }
  }
}
