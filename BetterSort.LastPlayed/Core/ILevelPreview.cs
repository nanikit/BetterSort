#nullable enable
namespace BetterSort.LastPlayed.Core {
  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }
  }
}
