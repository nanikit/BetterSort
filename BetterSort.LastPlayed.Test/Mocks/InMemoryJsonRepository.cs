using BetterSort.LastPlayed.External;

namespace BetterSort.LastPlayed.Test.Mocks {

  internal class InMemoryPlayedDateJsonRepository : IHistoryJsonRepository {
    public string? Json { get; set; }
    public string? BackupJson { get; set; }
    public string? PlayHistoryJson { get; set; }

    public string? Load() {
      return Json;
    }

    public string? LoadPlayHistory() {
      return PlayHistoryJson;
    }

    public void Save(string json) {
      Json = json;
    }

    public void SaveBackup(string json) {
      BackupJson = json;
    }
  }
}
