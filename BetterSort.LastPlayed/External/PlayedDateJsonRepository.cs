using System;
using System.IO;

namespace BetterSort.LastPlayed.External {

  public interface IPlayedDateJsonRepository {

    string? Load();

    void Save(string json);
  }

  internal class PlayedDateJsonRepository : IPlayedDateJsonRepository {
    private static readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "LastPlayedDates.json.dat");

    public string? Load() {
      return File.Exists(_path) ? File.ReadAllText(_path) : null;
    }

    public void Save(string json) {
      File.WriteAllText(_path, json);
    }
  }
}
