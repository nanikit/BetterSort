using BetterSort.Common.Models;

namespace BetterSort.Accuracy.External {
  public record OnlineBestRecord(
    double Accuracy,
    RecordDifficulty Difficulty,
    string Mode,
    int Score,
    string SongHash
  );
}
