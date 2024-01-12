using BetterSort.Common.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.Accuracy.Sorter {

  internal class AccuracyLegendMaker {

    public static List<(string, int)> GetLegend(IEnumerable<string> levelIds, AccuracyComparer comparer) {
      var legend = new List<(string, int)>();
      var accuracies = GetAccuracies(levelIds, comparer);
      int count = Math.Min(accuracies.Count, BetterSongListConstants.MaxFineLegendCount);

      for (int i = 0; i < count; i++) {
        int index = (int)((double)i / count * accuracies.Count);
        double accuracy = accuracies[index];
        legend.Add(((accuracy * 100).ToString("00.00"), index));
      }

      if (accuracies.Count < levelIds.Count()) {
        legend.Add(("N/A", accuracies.Count));
      }

      return legend;
    }

    private static List<double> GetAccuracies(IEnumerable<string> levelIds, AccuracyComparer comparer) {
      var accuracies = new List<double>();
      foreach (string levelId in levelIds) {
        if (comparer.GetAccuracy(levelId) is double accuracy) {
          accuracies.Add(accuracy);
        }
      }
      return accuracies;
    }
  }
}
