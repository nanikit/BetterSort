namespace BetterSongList.LastPlayedSort.Test.Mocks {
  using BetterSongList.LastPlayedSort.External;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal class InMemoryDateRepository : IPlayedDateRepository {
    public Dictionary<string, DateTime> LastPlayedDate { get; set; } = new();

    public StoredData? Load() {
      return new StoredData() {
        Version = "",
        LastPlays = LastPlayedDate,
      };
    }

    public void Save(IDictionary<string, DateTime> playDates) {
      LastPlayedDate = playDates.ToDictionary(x => x.Key, x => x.Value);
    }
  }
}
