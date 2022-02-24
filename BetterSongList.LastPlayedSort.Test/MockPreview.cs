using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterSongList.LastPlayedSort.Test {

  class MockPreview : IPreviewBeatmapLevel {
    public MockPreview(string id) {
      levelID = id;
    }

    public string levelID { get; private set; }

    public string songName => throw new NotImplementedException();

    public string songSubName => throw new NotImplementedException();

    public string songAuthorName => throw new NotImplementedException();

    public string levelAuthorName => throw new NotImplementedException();

    public float beatsPerMinute => throw new NotImplementedException();

    public float songTimeOffset => throw new NotImplementedException();

    public float shuffle => throw new NotImplementedException();

    public float shufflePeriod => throw new NotImplementedException();

    public float previewStartTime => throw new NotImplementedException();

    public float previewDuration => throw new NotImplementedException();

    public float songDuration => throw new NotImplementedException();

    public EnvironmentInfoSO environmentInfo => throw new NotImplementedException();

    public EnvironmentInfoSO allDirectionsEnvironmentInfo => throw new NotImplementedException();

    public PreviewDifficultyBeatmapSet[] previewDifficultyBeatmapSets => throw new NotImplementedException();

    public Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
  }
}
