using BetterSort.Accuracy.External;
using BetterSort.Common.External;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {

  internal class MockBsInterop : IBsInterop {

    public event Action<PlayRecord> OnSongPlayed = delegate { };

    public void Dispose() {
    }

    public Task SetModeAndDifficulty(BeatmapCharacteristicSO mode, RecordDifficulty difficulty) {
      return Task.CompletedTask;
    }

    public void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels) {
    }
  }
}
