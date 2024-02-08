using BetterSort.Accuracy.External;
using System;
using System.Collections.Generic;

namespace BetterSort.Accuracy.Test.Mocks {

  internal class MockBsInterop : IBsInterop {

    public event Action<PlayRecord> OnSongPlayed = delegate { };

    public void Dispose() {
    }

    public void SetPlaylistItem(IReadOnlyCollection<IPreviewBeatmapLevel> levels) {
    }
  }
}
