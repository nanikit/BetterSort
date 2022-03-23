#nullable enable
namespace BetterSongList.LastPlayedSort.Core {
  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }
  }
}
