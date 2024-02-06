using SiraUtil.Logging;
using System;
using System.Collections.Generic;

namespace BetterSort.LastPlayed.External {
  public class ImmigrationRepository {
    private readonly SongPlayHistoryImporter _importer;
    private readonly SiraLog _logger;
    private readonly IPlayedDateRepository _repository;

    internal ImmigrationRepository(SiraLog logger, IPlayedDateRepository repository, SongPlayHistoryImporter importer) {
      _logger = logger;
      _repository = repository;
      _importer = importer;
    }

    public StoredData? Load() {
      return _repository.Load() ?? TryImportingSongPlayHistoryData();
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      _repository.Save(playDates);
    }

    private StoredData? TryImportingSongPlayHistoryData() {
      var history = _importer.Load();
      if (history == null) {
        _logger.Debug("Searched SongPlayHistory data but not found. No history restored.");
        return null;
      }

      _logger.Debug($"Imported SongPlayHistory data, total count: {history.Count}");
      return new StoredData() { LastPlays = history };
    }
  }
}
