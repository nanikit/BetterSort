namespace BetterSort.Accuracy.External {
  using BetterSort.Accuracy.Sorter;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public interface IAccuracyRepository {
    Task Save(IReadOnlyDictionary<string, double> accuracies);

    Task<StoredData?> Load();
  }

  internal class AccuracyRepository : IAccuracyRepository {
    public AccuracyRepository(IPALogger logger) {
      _logger = logger;
    }

    public Task Save(IReadOnlyDictionary<string, double> accuracies) {
      var sorted = new SortedDictionary<string, double>(
        accuracies.ToDictionary(x => x.Key, x => x.Value),
        new AccuracyComparer(accuracies)
      );
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(AccuracyRepository).Assembly.GetName().Version}",
        BestAccuracies = sorted,
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
        _logger.Info($"Loaded {data.BestAccuracies?.Count.ToString() ?? "no"} records");
        return Task.FromResult<StoredData?>(data);
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return Task.FromResult<StoredData?>(null);
      }
    }

    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "BestAccuracies.json.dat");
    private readonly IPALogger _logger;
  }
}
