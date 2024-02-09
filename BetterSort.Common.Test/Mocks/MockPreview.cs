using BetterSort.Common.Models;
using Moq;

namespace BetterSort.Common.Test.Mocks {

  public class MockPreview : ILevelPreview {
    private readonly string _id;

    public MockPreview(string id) {
      _id = id;
    }

    public string LevelId => _id;

    public string SongName => _id;

    public static IPreviewBeatmapLevel GetMockPreviewBeatmapLevel(string id) {
      var mock = new Mock<IPreviewBeatmapLevel>();
      mock.Setup(mock => mock.levelID).Returns(id);
      mock.Setup(mock => mock.songName).Returns(id);
      return mock.Object;
    }

    public ILevelPreview Clone() {
      return new MockPreview(_id);
    }
  }
}
