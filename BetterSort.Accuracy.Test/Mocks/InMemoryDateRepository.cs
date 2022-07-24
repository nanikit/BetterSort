namespace BetterSort.LastPlayed.Test.Mocks {
  using BetterSort.LastPlayed.External;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal class InMemoryDateRepository : IPlayedDateRepository {
    public Dictionary<string, DateTime>? LastPlayedDate { get; set; } = new();

    public StoredData? Load() {
      return LastPlayedDate == null ? null : new StoredData() {
        Version = "",
        LastPlays = LastPlayedDate,
      };
    }

    public void Save(IReadOnlyDictionary<string, DateTime> playDates) {
      LastPlayedDate = playDates.ToDictionary(x => x.Key, x => x.Value);
    }
  }
}
