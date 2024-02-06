using BetterSort.LastPlayed.Sorter;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.External {

  public interface IPlayedDateRepository {

    StoredData? Load();

    void Save(IReadOnlyDictionary<string, DateTime> playDates);
  }

  public class PlayedDateRepository : IPlayedDateRepository {
    private readonly SiraLog _logger;
    private readonly IPlayedDateJsonRepository _jsonRepository;

    public PlayedDateRepository(SiraLog logger, IPlayedDateJsonRepository jsonRepository) {
      _logger = logger;
      _jsonRepository = jsonRepository;
    }

    public StoredData? Load() {
      try {
        string? json = _jsonRepository.Load();
        if (json == null) {
          _logger.Debug($"Attempting to load, but there is no existing play history.");
          return null;
        }

        var data = JsonConvert.DeserializeObject<StoredData>(json);
        _logger.Info($"Loaded {data?.LastPlays?.Count.ToString() ?? "no"} records");
        return data;
      }
      catch (Exception exception) {
        _logger.Error(exception);
        return null;
      }
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      var sorted = new SortedDictionary<string, DateTime>(
        playDates.ToDictionary(x => x.Key, x => x.Value),
        new LastPlayedDateComparer(playDates)
      );
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = $"{typeof(PlayedDateRepository).Assembly.GetName().Version}",
        LastPlays = sorted,
      }, Formatting.Indented);

      try {
        _jsonRepository.Save(json);
        _logger.Info($"Saved {playDates.Count} records");
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }
  }
}
