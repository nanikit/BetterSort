namespace BetterSongList.LastPlayedSort.Test {
  using BetterSongList.LastPlayedSort.Core;

  class MockPreview : ILevelPreview {
    public string LevelId => _id;

    public string SongName => _id;

    public MockPreview(string id) {
      _id = id;
    }

    private readonly string _id;
  }
}
