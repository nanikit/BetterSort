namespace BetterSort.Accuracy.Test.Mocks {
  using BetterSort.Accuracy.External;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  internal class InMemoryDateRepository : IAccuracyRepository {
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> BestAccuracies { get; set; } = new();
    public DateTime? LastRecordedAt;

    public Task Save(Dictionary<string, Dictionary<string, Dictionary<string, double>>> accuracies) {
      BestAccuracies = accuracies.ToDictionary(x => x.Key, x => x.Value);
      return Task.CompletedTask;
    }

    Task<StoredData?> IAccuracyRepository.Load() {
      return Task.FromResult(BestAccuracies == null ? null : new StoredData() {
        Version = "",
        LastRecordAt = LastRecordedAt ?? DateTime.MinValue,
      });
    }
  }
}
