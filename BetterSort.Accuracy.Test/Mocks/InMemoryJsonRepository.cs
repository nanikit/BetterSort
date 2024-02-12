using BetterSort.Accuracy.External;

namespace BetterSort.Accuracy.Test.Mocks {

  internal class InMemoryJsonRepository : IAccuracyJsonRepository {
    public string? Json { get; set; }

    public string? Load() {
      return Json;
    }

    public void Save(string json) {
      Json = json;
    }
  }
}
