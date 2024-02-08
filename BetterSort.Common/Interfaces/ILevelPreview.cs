namespace BetterSort.Common.Interfaces {

  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }

    ILevelPreview Clone();
  }

  public class LevelPreview(IPreviewBeatmapLevel preview) : ILevelPreview {
    public IPreviewBeatmapLevel Preview { get => preview; }
    public string LevelId { get => preview.levelID; }
    public string SongName { get => preview.songName; }

    ILevelPreview ILevelPreview.Clone() {
      return new LevelPreview(preview);
    }
  }
}
