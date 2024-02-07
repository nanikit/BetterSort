using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.External {

  public class ImmigrationRepository {
    private readonly SongPlayHistoryImporter _importer;
    private readonly SiraLog _logger;
    private readonly PlayedDateRepository _repository;

    internal ImmigrationRepository(SiraLog logger, PlayedDateRepository repository, SongPlayHistoryImporter importer) {
      _logger = logger;
      _repository = repository;
      _importer = importer;
    }

    public StoredData? Load() {
      return _repository.Load() ?? TryImportingSongPlayHistoryData();
    }

    public void Save(IReadOnlyList<LastPlayRecord> playDates) {
      _repository.Save(playDates);
    }

    private StoredData? TryImportingSongPlayHistoryData() {
      var history = _importer.Load();
      if (history == null) {
        _logger.Debug("Searched SongPlayHistory data but not found. No history restored.");
        return null;
      }

      _logger.Debug($"Imported SongPlayHistory data, total count: {history.Count}");
      return new StoredData() { LatestRecords = history.Select(x => new LastPlayRecord(x.Value, x.Key, null)).ToList() };
    }
  }
}
