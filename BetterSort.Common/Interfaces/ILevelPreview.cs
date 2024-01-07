#nullable enable

namespace BetterSort.Common.Interfaces {

  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }

    ILevelPreview Clone();
  }

  public class LevelPreview : ILevelPreview {

    public LevelPreview(IPreviewBeatmapLevel preview) {
      Preview = preview;
    }

    public string LevelId { get => Preview.levelID; }
    public IPreviewBeatmapLevel Preview { get; private set; }
    public string SongName { get => Preview.songName; }

    public ILevelPreview Clone() {
      return new LevelPreview(Preview);
    }
  }
}
