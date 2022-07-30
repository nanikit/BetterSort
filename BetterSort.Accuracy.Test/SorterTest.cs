namespace BetterSort.Accuracy.Test {
  using BetterSort.Accuracy.Sorter;
  using BetterSort.Accuracy.Test.Mocks;
  using BetterSort.Common.Compatibility;
  using BetterSort.Common.Interfaces;
  using BetterSort.Test.Common.Mocks;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Xunit;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class SorterTest {
    public SorterTest(IPALogger logger) {
      _logger = new MockLogger();

      var container = new DiContainer();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryDateRepository>().AsSingle();
      container.Bind<FilterSortAdaptor>().AsSingle();
    }

    // BetterSongList can pass empty list.
    [Fact]
    public void TestEmptySort() {
      //_sorter.LastPlayedDates = new();
      //var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      //_adaptor.DoSort(ref data, true);
      //Assert.Equal(0, data.Count());
    }

    //  [Fact]
    //  public async Task TestOneshotSort() {
    //    _clock.Now = new DateTime(2022, 3, 1);

    //    var data = GenerateData().ToList();
    //    _sorter.LastPlayedDates = data.ToDictionary(x => x.preview.LevelId, x => 0.0);

    //    data.ShuffleInPlace();
    //    var result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);

    //    Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), result.Levels.Select(x => x.LevelId));
    //    var legend = result.Legend.ToList();
    //    _logger.Debug(legend.Aggregate("", (acc, x) => $"{acc}, {x}"));
    //    Assert.Equal(legend, new[] {
    //      ("00:00", 0), ("12:05", 35),
    //      (legend[2].Label, 47), (legend[3].Label, 57), (legend[4].Label, 67), (legend[5].Label, 78),
    //      ("02-20", 89), ("02-16", 102), ("02-11", 115), ("02-03", 130), ("01-23", 146), ("01-08", 165),
    //      ("12-17", 185), ("11-16", 208), ("10-03", 234), ("08-02", 263), ("05-07", 295), ("2021-01", 331),
    //      ("2020-07", 372), ("2019-11", 417), ("2018-11", 468), ("2017-07", 526), ("2015-08", 590),
    //      ("2012-12", 662), ("2009-03", 743), ("2003-10", 834), ("1996-03", 936),
    //    });
    //    Assert.True(legend.Count <= 28, "Too many legend");
    //  }


    private readonly IPALogger _logger;
    //  private readonly DiContainer _container;
    //  private readonly FilterSortAdaptor _adaptor;
    //  private readonly AccuracySorter _sorter;
    //  private readonly FixedClock _clock;

    //  private async Task<ISortFilterResult> WaitResult(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
    //    TaskCompletionSource<ISortFilterResult?> completer = new();
    //    void SetResult(ISortFilterResult? res) {
    //      completer.TrySetResult(res);
    //    }
    //    _sorter.OnResultChanged += SetResult;

    //    _sorter.NotifyChange(newLevels, isSelected, token);

    //    var result = await completer.Task.ConfigureAwait(false);
    //    _sorter.OnResultChanged -= SetResult;
    //    if (result == null) {
    //      Assert.Fail("result is null");
    //      throw new Exception();
    //    }

    //    return result;
    //  }


    //  private IEnumerable<(MockPreview preview, DateTime date)> GenerateData() {
    //    return Enumerable.Range(0, 1000)
    //      .Select(i => (
    //        preview: new MockPreview($"{i}"),
    //        date: _clock.Now.AddSeconds(-Math.Pow(i, 3))));
    //  }
  }
}
