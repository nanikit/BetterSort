using System;
using System.IO;

namespace BetterSort.Accuracy.External {

  public interface IAccuracyJsonRepository {

    string? Load();

    void Save(string json);
  }

  internal class AccuracyJsonRepository : IAccuracyJsonRepository {
    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "UserData", "BestAccuracies.json.dat");

    public string? Load() {
      return ReadAllTextOrNull(_path);
    }

    public void Save(string json) {
      File.WriteAllText(_path, json);
    }

    private static string? ReadAllTextOrNull(string path) {
      return File.Exists(path) ? File.ReadAllText(path) : null;
    }
  }
}
