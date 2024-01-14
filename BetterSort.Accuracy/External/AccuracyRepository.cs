using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public interface IAccuracyRepository {

    Task Save(BestRecords accuracies);

    Task<StoredData?> Load();
  }

  public class AccuracyRepository : IAccuracyRepository {
    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "BestAccuracies.json.dat");

    private readonly SiraLog _logger;

    private readonly IClock _clock;

    private StoredData? _cache;

    public AccuracyRepository(SiraLog logger, IClock clock) {
      _logger = logger;
      _clock = clock;
    }

    public Task Save(BestRecords accuracies) {
      var sorted = new SortedDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>(
        accuracies,
        new AccuracyComparer(accuracies) { IsDescending = true }
      );
      var now = _clock.Now;
      _cache = new StoredData() {
        Version = $"{typeof(AccuracyRepository).Assembly.GetName().Version}",
        BestRecords = sorted,
        LastRecordAt = now,
      };
      string json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions() { WriteIndented = true });
      File.WriteAllText(_path, json);
      _logger.Info($"Saved {accuracies.Count} records");

      return Task.CompletedTask;
    }

    public Task<StoredData?> Load() {
      if (_cache != null) {
        _logger.Debug($"{nameof(AccuracyRepository)}.{nameof(Load)}: return cache.");
        return Task.FromResult<StoredData?>(_cache);
      }

      if (!File.Exists(_path)) {
        _logger.Debug($"Try loading but play history is not exist.");
        return Task.FromResult<StoredData?>(null);
      }

      try {
        string json = File.ReadAllText(_path);
        _cache = JsonSerializer.Deserialize<StoredData>(json);
        _logger.Info($"Loaded {_cache!.BestRecords?.Count.ToString() ?? "no"} records");
        return Task.FromResult<StoredData?>(_cache);
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return Task.FromResult<StoredData?>(null);
      }
    }
  }
}
