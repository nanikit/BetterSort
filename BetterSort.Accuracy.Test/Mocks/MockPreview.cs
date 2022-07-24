namespace BetterSort.LastPlayed.Test {
  using BetterSort.Common.Interfaces;

  class MockPreview : ILevelPreview {
    public string LevelId => _id;

    public string SongName => _id;

    public MockPreview(string id) {
      _id = id;
    }

    private readonly string _id;
  }
}
