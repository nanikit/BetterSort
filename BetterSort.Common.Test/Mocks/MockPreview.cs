using BetterSort.Common.Models;
using Moq;

namespace BetterSort.Common.Test.Mocks {

  public class MockPreview(string id) : ILevelPreview {
    public string LevelId => id;

    public string SongName => id;

    public static BaseBeatmapLevel GetMockPreviewBeatmapLevel(string id) {
#if NOT_BEFORE_1_36_2
      var mock = new Mock<BaseBeatmapLevel>([false, id, id, id, id, new string[] { }, new string[] { }, 0, 0, 0, 0, 0, 0, null, null, null]);
#else
      var mock = new Mock<BaseBeatmapLevel>();
      mock.Setup(mock => mock.levelID).Returns(id);
      mock.Setup(mock => mock.songName).Returns(id);
#endif
      return mock.Object;
    }

    public ILevelPreview Clone() {
      return new MockPreview(id);
    }
  }
}
