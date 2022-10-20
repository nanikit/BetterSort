using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.External {
  using BestRecords = Dictionary<string, Dictionary<string, Dictionary<string, double>>>;

  public interface IAccuracyRepository {
    Task Save(BestRecords accuracies);

    Task<StoredData?> Load();
  }

  internal class AccuracyRepository : IAccuracyRepository {
    public AccuracyRepository(IPALogger logger, IClock clock) {
      _logger = logger;
      _clock = clock;
    }

    public Task Save(BestRecords accuracies) {
      var sorted = new SortedDictionary<string, Dictionary<string, Dictionary<string, double>>>(
        accuracies,
        new BeatmapAccuracyComparer(accuracies)
      );
      var now = _clock.Now;
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(AccuracyRepository).Assembly.GetName().Version}",
        BestRecords = sorted,
        LastRecordAt = now,
      }, Formatting.Indented);
      File.WriteAllText(_path, json);
      _logger.Info($"Saved {accuracies.Count} records");
      return Task.CompletedTask;
    }

    public Task<StoredData?> Load() {
      if (!File.Exists(_path)) {
        _logger.Debug($"Try loading but play history is not exist.");
        return Task.FromResult<StoredData?>(null);
      }

      try {
        string json = File.ReadAllText(_path);
        var data = JsonConvert.DeserializeObject<StoredData>(json);
        _logger.Info($"Loaded {data.BestRecords?.Count.ToString() ?? "no"} records");
        return Task.FromResult<StoredData?>(data);
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return Task.FromResult<StoredData?>(null);
      }
    }

    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "BestAccuracies.json.dat");
    private readonly IPALogger _logger;
    private readonly IClock _clock;
  }
}
