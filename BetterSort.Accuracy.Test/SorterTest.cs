namespace BetterSort.Accuracy.Test {
  using BetterSort.Accuracy.External;
  using BetterSort.Accuracy.Sorter;
  using BetterSort.Accuracy.Test.Mocks;
  using BetterSort.Common.Compatibility;
  using BetterSort.Test.Common.Mocks;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using Xunit;
  using Xunit.Abstractions;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class SorterTest {
    private readonly IPALogger _logger;
    private readonly DiContainer _container;
    private readonly FilterSortAdaptor _adaptor;
    private readonly AccuracySorter _sorter;

    public SorterTest(ITestOutputHelper output) {
      _logger = new MockLogger(output);

      var container = new DiContainer();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(_logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryDateRepository>().AsSingle();
      container.Bind<FilterSortAdaptor>().AsSingle();
      _container = container;


    }

    // BetterSongList can pass empty list.
    [Fact]
    public void TestEmptySort() {
      _sorter.BestRecords = new();
      var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      _adaptor.DoSort(ref data, true);
      Assert.Empty(data);
    }

    [Fact]
    public void TestSort() {
      string json = JsonConvert.SerializeObject(new BestRecord() {
        Difficulty = BeatmapDifficulty.Hard,
      });
      var record = JsonConvert.DeserializeObject<BestRecord>(json, new StringEnumConverter());
      Debug.WriteLine(json);
    }
  }
}
