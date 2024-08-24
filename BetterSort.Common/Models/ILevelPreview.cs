namespace BetterSort.Common.Models {

  public interface ILevelPreview {
    string LevelId { get; }
    string SongName { get; }

    ILevelPreview Clone();
  }

  public class LevelPreview(BaseBeatmapLevel preview) : ILevelPreview {
    public BaseBeatmapLevel Preview { get => preview; }
    public string LevelId { get => preview.levelID; }
    public string SongName { get => preview.songName; }

    ILevelPreview ILevelPreview.Clone() {
      return new LevelPreview(preview);
    }
  }
}
