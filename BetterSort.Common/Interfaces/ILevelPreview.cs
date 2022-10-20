#nullable enable
namespace BetterSort.Common.Interfaces {
  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }

    ILevelPreview Clone();
  }

  public class LevelPreview : ILevelPreview {
    public IPreviewBeatmapLevel Preview { get; private set; }

    public string LevelId { get => Preview.levelID; }
    public string SongName { get => Preview.songName; }

    public LevelPreview(IPreviewBeatmapLevel preview) {
      Preview = preview;
    }

    public ILevelPreview Clone() {
      return new LevelPreview(Preview);
    }
  }
}
