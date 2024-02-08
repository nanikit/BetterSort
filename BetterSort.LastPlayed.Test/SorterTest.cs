using BetterSort.Common.Compatibility;
using BetterSort.Common.Interfaces;
using BetterSort.Common.Test;
using BetterSort.Common.Test.Mocks;
using BetterSort.LastPlayed.External;
using BetterSort.LastPlayed.Installers;
using BetterSort.LastPlayed.Sorter;
using BetterSort.LastPlayed.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace BetterSort.LastPlayed.Test {

  [TestClass]
  public class SorterTest {
    private readonly FilterSortAdaptor _adaptor;
    private readonly FixedClock _clock;
    private readonly DiContainer _container;
    private readonly MockEventSource _playSource;
    private readonly InMemoryPlayedDateJsonRepository _repository;
    private readonly LastPlayedDateSorter _sorter;

    public SorterTest() {
      DiContainer container = new();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<MockEventSource>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryPlayedDateJsonRepository>().AsSingle();

      container.Install<SorterInstaller>();
      _clock = container.Resolve<FixedClock>();
      _playSource = container.Resolve<MockEventSource>();
      _sorter = container.Resolve<LastPlayedDateSorter>();
      _repository = container.Resolve<InMemoryPlayedDateJsonRepository>();
      _adaptor = container.Resolve<FilterSortAdaptor>();
      _container = container;
    }

    // BetterSongList can pass empty list.
    [TestMethod]
    public void TestEmptySort() {
      _sorter.LastPlays = new();
      var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      _adaptor.DoSort(ref data, true);
      Assert.AreEqual(0, data.Count());
    }

    [TestMethod]
    public async Task TestOneOffSort() {
      _clock.Now = new DateTime(2022, 3, 1);

      var data = GenerateData().ToList();
      _sorter.LastPlays = data.ToDictionary(x => x.preview.LevelId, x => new LevelPlayData(x.date, null));

      var random = new Random(30000);
      data = [.. data.OrderBy(x => random.Next())];
      var result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);

      CollectionAssert.AreEqual(
        Enumerable.Range(0, 1000).Select(i => $"{i}").ToList(),
        result.Levels.Select(x => x.LevelId).ToList()
      );

      var legend = result.Legend.ToList();
      CollectionAssert.AreEqual(new[] {
        ("00:00", 0), ("12:05", 35),
        (legend[2].Label, 47), (legend[3].Label, 57), (legend[4].Label, 67), (legend[5].Label, 78),
        ("02-20", 89), ("02-16", 102), ("02-11", 115), ("02-03", 130), ("01-23", 146), ("01-08", 165),
        ("12-17", 185), ("11-16", 208), ("10-03", 234), ("08-02", 263), ("05-07", 295), ("2021-01", 331),
        ("2020-07", 372), ("2019-11", 417), ("2018-11", 468), ("2017-07", 526), ("2015-08", 590),
        ("2012-12", 662), ("2009-03", 743), ("2003-10", 834), ("1996-03", 936),
      }, legend);
      Assert.IsTrue(legend.Count <= 28, "Too many legend");
    }

    // TODO: Test short play skip
    [TestMethod]
    public async Task TestSort() {
      _clock.Now = new DateTime(2022, 3, 1);
      var data = GenerateData().ToList();
      _repository.Json = JsonConvert.SerializeObject(new StoredData() {
        LatestRecords = data.Select(x => new LastPlayRecord(x.date, x.preview.LevelId, null)).ToList(),
      });

      _container.Resolve<SorterEnvironment>().Initialize();

      var result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);

      CollectionAssert.AreEqual(
        Enumerable.Range(0, 1000).Select(i => $"{i}").ToList(),
        result.Levels.Select(x => x.LevelId).ToList()
      );

      _clock.Now = new DateTime(2022, 3, 1, 7, 0, 0);
      _playSource.SimulatePlay(new LastPlayRecord(_clock.Now, "1", null));
      result = await WaitResult(data.Select(x => x.preview), true).ConfigureAwait(false);
      var levels = result.Levels.ToList();

      var expectation = new List<int>() { 1, 0 }.Concat(Enumerable.Range(2, 998)).Select(i => $"{i}");
      CollectionAssert.AreEqual(expectation.ToList(), levels.Select(x => x.LevelId).ToList());
    }

    private IEnumerable<(MockPreview preview, DateTime date)> GenerateData() {
      return Enumerable.Range(0, 1000)
        .Select(i => (
          preview: new MockPreview($"{i}"),
          date: _clock.Now.AddSeconds(-Math.Pow(i, 3))));
    }

    private async Task<ISortFilterResult> WaitResult(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false) {
      TaskCompletionSource<ISortFilterResult?> completer = new();
      void SetResult(ISortFilterResult? res) {
        completer.TrySetResult(res);
      }
      _sorter.OnResultChanged += SetResult;

      _sorter.NotifyChange(newLevels, isSelected);

      var result = await completer.Task.ConfigureAwait(false);
      _sorter.OnResultChanged -= SetResult;
      if (result == null) {
        Assert.Fail("result is null");
        throw new Exception();
      }

      return result;
    }
  }
}
