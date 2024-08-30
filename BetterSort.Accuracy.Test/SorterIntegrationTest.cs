using BetterSongList.SortModels;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Test;
using BetterSort.Common.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BetterSort.Accuracy.Test {

  [TestClass]
  public class SorterIntegrationTest {
    private readonly ISorterCustom _adaptor;
    private readonly ISorterWithLegend _legend;
    private readonly InMemoryJsonRepository _repository;

    public SorterIntegrationTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<InMemoryJsonRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<MockBsInterop>().AsSingle();
      container.Install<SorterInstaller>();

      _adaptor = container.Resolve<ISorterCustom>();
      _legend = container.Resolve<ISorterWithLegend>();
      _repository = container.Resolve<InMemoryJsonRepository>();
    }

    // BetterSongList can pass empty list.
    [TestMethod]
    public void TestEmptyCase() {
      var data = new List<BaseBeatmapLevel>().AsEnumerable();

      _adaptor.DoSort(ref data, true);

      CollectionAssert.AreEqual(new List<BaseBeatmapLevel>(), data.ToList());
    }

    [TestMethod]
    public void TestComplexCase() {
      _repository.Json = """
{
  "lastRecordAt": "2021-08-01T00:00:00Z",
  "bestRecords": [
    { levelId: "custom_level_1111111111111111111111111111111111111111", type: "Standard", difficulty: "ExpertPlus", accuracy: 0.90292 }
  ]
}
""";
      var input = new BaseBeatmapLevel[] {
        MockPreview.GetMockPreviewBeatmapLevel("custom_level_2222222222222222222222222222222222222222"),
        MockPreview.GetMockPreviewBeatmapLevel("custom_level_1111111111111111111111111111111111111111"),
        MockPreview.GetMockPreviewBeatmapLevel("custom_level_3333333333333333333333333333333333333333"),
      }.AsEnumerable();

      _adaptor.DoSort(ref input, true);
      var legend = _legend.BuildLegend(input.ToArray());

      CollectionAssert.AreEqual(new List<string> {
        "custom_level_1111111111111111111111111111111111111111",
        "custom_level_2222222222222222222222222222222222222222",
        "custom_level_3333333333333333333333333333333333333333",
      }, input.Select(x => x.levelID).ToList());
      CollectionAssert.AreEqual(new List<KeyValuePair<string, int>>() { new("90.29", 0), new("N/A", 1) }, legend.ToList());
    }
  }
}
