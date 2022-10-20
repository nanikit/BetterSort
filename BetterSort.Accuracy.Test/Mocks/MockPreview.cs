namespace BetterSort.Accuracy.Test.Mocks {
  using BetterSort.Common.Interfaces;

  class MockPreview : ILevelPreview {
    public string LevelId => _id;

    public string SongName => _id;

    public MockPreview(string id) {
      _id = id;
    }

    public ILevelPreview Clone() {
      return new MockPreview(_id);
    }

    private readonly string _id;
  }
}
