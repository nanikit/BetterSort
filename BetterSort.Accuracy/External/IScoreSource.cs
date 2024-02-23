using BetterSort.Common.Models;
using System.Collections.Generic;

namespace BetterSort.Accuracy.External {
  public record OnlineBestRecord(
    double Accuracy,
    RecordDifficulty Difficulty,
    string Mode,
    int Score,
    string SongHash
  );

  public record PagingMetadata(int Page, int ItemsPerPage, int Total);

  public interface IScoreSource {

    string GetRecordUrl(string playerId, int page);

    (List<OnlineBestRecord> Records, PagingMetadata Paging, string? Log) ToBestRecords(string json);
  }
}
