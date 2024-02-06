using BetterSort.Common.External;

namespace BetterSort.Accuracy.External {

  public class BestRecord {
    public double Accuracy { get; set; }
    public RecordDifficulty Difficulty { get; set; }
    public string Mode { get; set; } = "";
    public int Score { get; set; }
    public string SongHash { get; set; } = "";
  }
}
