namespace BetterSongList.LastPlayedSort.Test {
  using BetterSongList.LastPlayedSort.Compatibility;
  using BetterSongList.LastPlayedSort.Core;
  using BetterSongList.LastPlayedSort.Sorter;
  using BetterSongList.LastPlayedSort.Test.Mocks;
  using Nanikit.Test;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Xunit;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  internal class SorterTest {
    public SorterTest(IPALogger logger) {
      _logger = logger;

      DiContainer container = new();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<MockEventSource>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryDateRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<LastPlayedDateSorter>().AsSingle();
      container.Bind<FilterSortAdaptor>().AsSingle();
      container.Bind<SorterEnvironment>().AsSingle();
      _clock = container.Resolve<FixedClock>();
      _playSource = container.Resolve<MockEventSource>();
      _sorter = container.Resolve<LastPlayedDateSorter>();
      _repository = container.Resolve<InMemoryDateRepository>();
      _container = container;
    }

    [Test]
    public async Task TestOneshotSort() {
      _clock.Now = new DateTime(2022, 3, 1);

      var data = GenerateData().ToList();
      _sorter.LastPlayedDates = data.ToDictionary(x => x.preview.LevelId, x => x.date);

      data.ShuffleInPlace();
      ISortFilterResult? result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);

      Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), result.Levels.Select(x => x.LevelId));
      var legend = result.Legend.ToList();
      _logger.Debug(legend.Aggregate("", (acc, x) => $"{acc}, {x}"));
      Assert.Equal(legend, new[] {
        ("00:00", 0), ("12:05", 35),
        (legend[2].Label, 47), (legend[3].Label, 57), (legend[4].Label, 67), (legend[5].Label, 78),
        ("02-20", 89), ("02-16", 102), ("02-11", 115), ("02-03", 130), ("01-23", 146), ("01-08", 165),
        ("12-17", 185), ("11-16", 208), ("10-03", 234), ("08-02", 263), ("05-07", 295), ("2021-01", 331),
        ("2020-07", 372), ("2019-11", 417), ("2018-11", 468), ("2017-07", 526), ("2015-08", 590),
        ("2012-12", 662), ("2009-03", 743), ("2003-10", 834), ("1996-03", 936),
      });
      Assert.True(legend.Count <= 28, "Too many legend");
    }

    // TODO: BetterSongList passes empty list before passing real items. discuss or test it.
    // TODO: Test short play skip
    [Test]
    public async Task TestSort() {
      _clock.Now = new DateTime(2022, 3, 1);
      var data = GenerateData().ToList();
      _repository.LastPlayedDate = data.ToDictionary(x => x.preview.LevelId, x => x.date);

      _container.Resolve<SorterEnvironment>().Start(false);

      ISortFilterResult? result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);

      Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), result.Levels.Select(x => x.LevelId));

      _clock.Now = new DateTime(2022, 3, 1, 7, 0, 0);
      _playSource.SimulatePlay("1", _clock.Now);
      result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);
      var levels = result.Levels.ToList();

      IEnumerable<string> expectation = new List<int>() { 1, 0 }.Concat(Enumerable.Range(2, 998)).Select(i => $"{i}");
      Assert.Equal(expectation, levels.Select(x => x.LevelId));
    }

    private readonly IPALogger _logger;
    private readonly DiContainer _container;
    private readonly MockEventSource _playSource;
    private readonly InMemoryDateRepository _repository;
    private readonly LastPlayedDateSorter _sorter;
    private readonly FixedClock _clock;

    private async Task<ISortFilterResult> WaitResult(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      TaskCompletionSource<ISortFilterResult?> completer = new();
      void SetResult(ISortFilterResult? res) {
        completer.TrySetResult(res);
      }
      _sorter.OnResultChanged += SetResult;

      _sorter.NotifyChange(newLevels, isSelected, token);

      ISortFilterResult? result = await completer.Task.ConfigureAwait(false);
      _sorter.OnResultChanged -= SetResult;
      if (result == null) {
        Assert.Fail("result is null");
        throw new Exception();
      }

      return result;
    }


    private IEnumerable<(MockPreview preview, DateTime date)> GenerateData() {
      return Enumerable.Range(0, 1000)
        .Select(i => (
          preview: new MockPreview($"{i}"),
          date: _clock.Now.AddSeconds(-Math.Pow(i, 3))));
    }
  }
}
