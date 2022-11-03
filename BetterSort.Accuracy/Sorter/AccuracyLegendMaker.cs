using BetterSort.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace BetterSort.Accuracy.Sorter {
  internal class AccuracyLegendMaker {
    public static List<(string, int)> GetLegend(
      IReadOnlyList<ILevelPreview> levels,
      IReadOnlyDictionary<ILevelPreview, LevelRecord> recordMap
    ) {
      var legend = new List<(string, int)>();

      int count = Math.Min(Math.Min(32, recordMap.Count), levels.Count);
      for (int i = 0; i < count; i++) {
        int index = (int)((double)i / count * count);
        var record = recordMap[levels[index]];
        legend.Add(((record.Accuracy * 100).ToString("00.00"), index));
      }
      legend.Add(("N/A", recordMap.Count));

      return legend;
    }
  }
}
