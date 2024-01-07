using System;
using System.Collections.Generic;

namespace BetterSort.Accuracy.Sorter {

  internal class AccuracyLegendMaker {

    public static List<(string, int)> GetLegend(List<LevelRecord> mapping, int length) {
      var legend = new List<(string, int)>();

      int count = Math.Min(mapping.Count, 26);
      for (int i = 0; i < count; i++) {
        int index = (int)((double)i / count * mapping.Count);
        var record = mapping[index];
        legend.Add(((record.Accuracy * 100).ToString("00.00"), index));
      }
      if (mapping.Count < length)
        legend.Add(("N/A", mapping.Count));

      return legend;
    }
  }
}
