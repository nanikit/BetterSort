using BetterSort.Common.Models;
using Moq;

#if NOT_BEFORE_1_36_2

using System.Reflection;
using System.Runtime.Serialization;

#else
#endif

namespace BetterSort.Common.Test.Mocks {

  public class MockPreview(string id) : ILevelPreview {
    public string LevelId => id;

    public string SongName => id;

    public static BaseBeatmapLevel GetMockPreviewBeatmapLevel(string id) {
#if NOT_BEFORE_1_36_2
      var mock = new Mock<BaseBeatmapLevel>([false, id, id, id, id, new string[] { }, new string[] { }, 0, 0, 0, 0, 0, 0, PlayerSensitivityFlag.Unknown, null, null]);
      var obj = FormatterServices.GetSafeUninitializedObject(typeof(BaseBeatmapLevel)) as BaseBeatmapLevel;
      typeof(BaseBeatmapLevel).GetField("levelID", BindingFlags.Instance | BindingFlags.Public).SetValue(obj, id);
      typeof(BaseBeatmapLevel).GetField("songName", BindingFlags.Instance | BindingFlags.Public).SetValue(obj, id);
      return obj;
#else
      var mock = new Mock<BaseBeatmapLevel>();
      mock.Setup(mock => mock.levelID).Returns(id);
      mock.Setup(mock => mock.songName).Returns(id);
      return mock.Object;
#endif
    }

    public ILevelPreview Clone() {
      return new MockPreview(id);
    }
  }
}
