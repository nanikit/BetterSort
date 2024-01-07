using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Sorter;
using BetterSort.Common.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {

  internal class InMemoryRepository : IAccuracyRepository {
    public DateTime? LastRecordedAt;

    private readonly IClock _clock;

    public InMemoryRepository(IClock clock) {
      _clock = clock;
    }

    public Dictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>> BestAccuracies { get; set; } = new();

    public Task Save(IDictionary<string, Dictionary<string, Dictionary<RecordDifficulty, double>>> accuracies) {
      BestAccuracies = accuracies.ToDictionary(x => x.Key, x => x.Value);
      LastRecordedAt = _clock.Now;
      return Task.CompletedTask;
    }

    Task<StoredData?> IAccuracyRepository.Load() {
      return Task.FromResult(BestAccuracies == null ? null : new StoredData() {
        Version = "",
        LastRecordAt = LastRecordedAt ?? DateTime.MinValue,
        BestRecords = BestAccuracies,
      });
    }
  }
}
