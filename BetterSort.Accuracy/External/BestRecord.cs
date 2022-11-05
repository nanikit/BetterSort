namespace BetterSort.Accuracy.External {
  using BetterSort.Accuracy.Sorter;

  public class BestRecord {
    public string SongHash { get; set; } = "";

    public string Mode { get; set; } = "";

    public RecordDifficulty Difficulty { get; set; }

    public int Score { get; set; }

    public double Accuracy { get; set; }
  }
}

