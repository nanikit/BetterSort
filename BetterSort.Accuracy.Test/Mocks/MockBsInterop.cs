using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Sorter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {
  internal class MockBsInterop : IBsInterop {
    public event Action<PlayRecord> OnSongPlayed = delegate { };
    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public void Dispose() {
    }

    public Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty) {
      return Task.CompletedTask;
    }

    public void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels) {
    }
  }
}
