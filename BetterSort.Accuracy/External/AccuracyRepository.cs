using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
using BetterSort.Common.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.External {

  public interface IAccuracyRepository {

    Task Save(BestRecords accuracies);

    Task<StoredData?> Load();
  }

  public class AccuracyRepository(SiraLog logger, IAccuracyJsonRepository jsonRepository, IClock clock) : IAccuracyRepository {
    private readonly SiraLog _logger = logger;
    private readonly IClock _clock = clock;
    private readonly IAccuracyJsonRepository _jsonRepository = jsonRepository;

    private StoredData? _cache;

    public Task Save(BestRecords accuracies) {
      var data = GetSortedData(accuracies);

      var indented = new JsonSerializerSettings() { Formatting = Formatting.Indented };
      string json = JsonConvert.SerializeObject(accuracies, indented);
      _jsonRepository.Save(json);

      _logger.Info($"Saved {accuracies.Count} records");

      _cache = BypassSortedDictionaryMemoryLeak(accuracies, data);

      return Task.CompletedTask;
    }

    public Task<StoredData?> Load() {
      if (_cache != null) {
        _logger.Trace($"Load from cache.");
        return Task.FromResult<StoredData?>(_cache);
      }

      try {
        string? json = _jsonRepository.Load();
        if (json == null) {
          _logger.Debug($"Attempted to load but play history is not exist.");
          return Task.FromResult<StoredData?>(null);
        }

        _cache = JsonConvert.DeserializeObject<StoredData>(json);
        _logger.Info($"Loaded {_cache!.BestRecords?.Count.ToString() ?? "no"} records");
        return Task.FromResult<StoredData?>(_cache);
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return Task.FromResult<StoredData?>(null);
      }
    }

    private StoredData BypassSortedDictionaryMemoryLeak(BestRecords accuracies, StoredData data) {
      data.BestRecords = accuracies;
      return data;
    }

    private StoredData GetSortedData(BestRecords accuracies) {
      var sorted = new SortedDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>>(
        accuracies,
        new AccuracyComparer(accuracies) { IsDescending = true }
      );

      var now = _clock.Now;
      var data = new StoredData() {
        Version = $"{typeof(AccuracyRepository).Assembly.GetName().Version}",
        BestRecords = sorted,
        LastRecordAt = now,
      };
      return data;
    }
  }
}
