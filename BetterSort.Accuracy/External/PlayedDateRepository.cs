namespace BetterSort.Accuracy.External {
  using BetterSort.Accuracy.Sorter;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using IPALogger = IPA.Logging.Logger;

  public interface IAccuracyRepository {
    void Save(IReadOnlyDictionary<string, double> accuracies);

    StoredData? Load();
  }

  internal class PlayedDateRepository : IAccuracyRepository {
    public PlayedDateRepository(IPALogger logger) {
      _logger = logger;
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      var sorted = new SortedDictionary<string, DateTime>(
        playDates.ToDictionary(x => x.Key, x => x.Value),
        new LastPlayedDateComparer(playDates)
      );
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(PlayedDateRepository).Assembly.GetName().Version}",
        BestAccuracies = sorted,
      }, Formatting.Indented);
      File.WriteAllText(_path, json);
      _logger.Info($"Saved {playDates.Count} records");
    }

    public StoredData? Load() {
      if (!File.Exists(_path)) {
        _logger.Debug($"Try loading but play history is not exist.");
        return null;
      }

      try {
        string json = File.ReadAllText(_path);
        var data = JsonConvert.DeserializeObject<StoredData>(json);
        _logger.Info($"Loaded {data.BestAccuracies?.Count.ToString() ?? "no"} records");
        return data;
      }
      catch (Exception exception) {
        _logger.Warn(exception);
        return null;
      }
    }

    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "LastPlayedDates.json.dat");
    private readonly IPALogger _logger;
  }
}
