using BetterSort.Common.Interfaces;

namespace BetterSort.Common.Test.Mocks {

  public class MockPreview : ILevelPreview {
    private readonly string _id;

    public MockPreview(string id) {
      _id = id;
    }

    public string LevelId => _id;

    public string SongName => _id;

    public ILevelPreview Clone() {
      return new MockPreview(_id);
    }
  }
}
