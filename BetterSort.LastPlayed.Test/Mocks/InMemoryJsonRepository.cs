using BetterSort.LastPlayed.External;

namespace BetterSort.LastPlayed.Test.Mocks {

  internal class InMemoryPlayedDateJsonRepository : IPlayedDateJsonRepository {
    public string? Json { get; set; }

    public string? Load() {
      return Json;
    }

    public void Save(string json) {
      Json = json;
    }
  }
}
