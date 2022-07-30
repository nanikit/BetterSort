namespace BetterSort.Accuracy.Test.Mocks {
  using BetterSort.Accuracy.External;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  internal class InMemoryDateRepository : IAccuracyRepository {
    public Dictionary<string, double>? BestAccuracies { get; set; } = new();

    public Task Save(IReadOnlyDictionary<string, double> accuracies) {
      BestAccuracies = accuracies.ToDictionary(x => x.Key, x => x.Value);
      return Task.CompletedTask;
    }

    Task<StoredData?> IAccuracyRepository.Load() {
      //return BestAccuracies == null ? null : new StoredData() {
      //  Version = "",
      //  LastPlays = BestAccuracies,
      //};
      return Task.FromResult<StoredData?>(new StoredData());
    }
  }
}
