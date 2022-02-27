namespace BetterSongList.LastPlayedSort.External {
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  internal interface IPlayedDateRepository {
    void Save(IEnumerable<(string, DateTime)> playDates);

    StoredData? Load();
  }

  internal class PlayedDateRepository : IPlayedDateRepository {
    public void Save(IEnumerable<(string, DateTime)> playDates) {
      string json = JsonConvert.SerializeObject(new StoredData() {
        Version = "",
        LastPlays = playDates.ToList(),
      });
      File.WriteAllText(_path, json);
    }

    public StoredData? Load() {
      if (!File.Exists(_path)) {
        return null;
      }

      string json = File.ReadAllText(_path);
      return JsonConvert.DeserializeObject<StoredData>(json);
    }

    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "LastPlayedDates.json");
  }
}
