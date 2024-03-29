using System;
using System.IO;

namespace BetterSort.LastPlayed.External {

  public interface IHistoryJsonRepository {

    string? Load();

    string? LoadPlayHistory();

    void Save(string json);

    void SaveBackup(string json);
  }

  internal class HistoryJsonRepository : IHistoryJsonRepository {
    private static readonly string _historyJsonPath = Path.Combine(Environment.CurrentDirectory, "UserData", "LastPlayedDates.json.dat");
    private static readonly string _historyJsonPathBackup = $"{_historyJsonPath}.bak";
    private static readonly string _sphJsonPath = Path.Combine(Environment.CurrentDirectory, "UserData", "SongPlayData.json");

    public string? Load() {
      return ReadAllTextOrNull(_historyJsonPath);
    }

    public void Save(string json) {
      File.WriteAllText(_historyJsonPath, json);
    }

    public void SaveBackup(string json) {
      File.WriteAllText(_historyJsonPathBackup, json);
    }

    public string? LoadPlayHistory() {
      return ReadAllTextOrNull(_sphJsonPath);
    }

    private static string? ReadAllTextOrNull(string path) {
      return File.Exists(path) ? File.ReadAllText(path) : null;
    }
  }
}
