global using SorterData = System.Collections.Generic.Dictionary<
  string,
  System.Collections.Generic.Dictionary<(string Type, BetterSort.Common.Models.RecordDifficulty), double>
>;
using BetterSort.Common.External;
using BetterSort.Common.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public interface IAccuracyRepository {

    Task Save(SorterData accuracies);

    Task<SorterData?> Load();
  }

  public class AccuracyRepository(SiraLog logger, IAccuracyJsonRepository jsonRepository, IClock clock) : IAccuracyRepository {
    private SorterData? _cache;

    public Task Save(SorterData accuracies) {
      var (json, data) = GetPersistentData(accuracies, clock.Now);
      jsonRepository.Save(json);

      logger.Info($"Saved {data.BestRecords.Count} records");
      _cache = accuracies;

      return Task.CompletedTask;
    }

    public Task<SorterData?> Load() {
      if (_cache != null) {
        logger.Trace($"Load from cache.");
        return Task.FromResult<SorterData?>(_cache);
      }

      try {
        string? json = jsonRepository.Load();
        if (json == null) {
          logger.Debug($"Attempted to load but play history is not exist.");
          return Task.FromResult<SorterData?>(null);
        }

        var data = JsonConvert.DeserializeObject<StoredData>(json);
        logger.Info($"Loaded {data?.BestRecords?.Count.ToString() ?? "no"} records");

        return Task.FromResult<SorterData?>(GetSorterData(data?.BestRecords));
      }
      catch (Exception exception) {
        logger.Warn(exception);
        return Task.FromResult<SorterData?>(null);
      }
    }

    internal static (string Json, StoredData Cache) GetPersistentData(SorterData accuracies, DateTime now) {
      var data = GetStoredData(accuracies, now);
      string json = SerializeRecords(data);
      return (json, data);
    }

    internal static SorterData GetSorterData(IEnumerable<BestRecord>? data) {
      var result = new SorterData();
      if (data == null) {
        return result;
      }

      foreach (var record in data) {
        AddIfBest(result, record);
      }
      return result;
    }

    internal static void AddIfBest(SorterData result, BestRecord record) {
      var (levelId, type, difficulty, accuracy) = record;
      var map = (type, difficulty);
      if (result.TryGetValue(levelId, out var level)) {
        if (level.TryGetValue(map, out double existingAccuracy)) {
          if (existingAccuracy < accuracy) {
            level[(type, difficulty)] = accuracy;
          }
        }
        else {
          level.Add((type, difficulty), accuracy);
        }
      }
      else {
        result.Add(record.LevelId, new Dictionary<(string Type, RecordDifficulty), double> { { map, accuracy } });
      }
    }

    private static StoredData GetStoredData(SorterData accuracies, DateTime now) {
      var bests = new SortedDictionary<BestRecord, object?>();
      foreach (var level in accuracies) {
        foreach (var record in level.Value) {
          var (type, difficulty) = record.Key;
          double accuracy = record.Value;
          bests.Add(new BestRecord(level.Key, type, difficulty, accuracy), null);
        }
      }

      return new StoredData() {
        Version = $"{typeof(AccuracyRepository).Assembly.GetName().Version}",
        BestRecords = [.. bests.Keys],
        LastRecordAt = now,
      };
    }

    private static string SerializeRecords(StoredData data) {
      return JsonConvert.SerializeObject(data, new JsonSerializerSettings() {
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Formatting = Formatting.Indented,
      });
    }
  }
}
