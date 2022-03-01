namespace BetterSongList.LastPlayedSort.Test {
  using BetterSongList.LastPlayedSort.Sorter;
  using BetterSongList.LastPlayedSort.Test.Mocks;
  using Nanikit.Test;
  using System;
  using System.Collections.Generic;
  using System.Linq;
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
      container.Bind<LastPlayedDateSorter>().AsSingle();
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

      bool isChangedFired = false;
      _sorter.ResultLevels.didChangeEvent += () => {
        isChangedFired = true;
      };

      var data = GenerateData().ToList();
      _sorter.LastPlayedDates = data.ToDictionary(x => x.preview.levelID, x => x.date);

      data.ShuffleInPlace();
      await _sorter.NotifyChange(data.Select(x => x.preview), true);

      var legend = _sorter.Legend.value.ToList();

      Assert.True(isChangedFired, "ResultLevels.didChangeEvent is not fired.");
      Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), _sorter.ResultLevels.value.Select(x => x.levelID));
      Assert.Equal("Yesterday", legend[0].Label);
      Assert.Equal(0, legend[0].Index);
      Assert.Equal("This week", legend[1].Label);
      Assert.Equal(45, legend[1].Index);
      Assert.Equal("6 years ago", legend[10].Label);
      Assert.Equal(456, legend[10].Index);
    }

    [Test]
    public async Task TestSort() {
      _clock.Now = new DateTime(2022, 3, 1);
      var data = GenerateData().ToList();
      _repository.LastPlayedDate = data.ToDictionary(x => x.preview.levelID, x => x.date);

      bool isChangedFired = false;
      _sorter.ResultLevels.didChangeEvent += () => {
        isChangedFired = true;
      };
      _container.Resolve<SorterEnvironment>().Start(false);
      await _sorter.NotifyChange(data.Select(x => x.preview), true);

      Assert.True(isChangedFired);
      Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), _sorter.ResultLevels.value.Select(x => x.levelID));

      isChangedFired = false;
      _clock.Now = new DateTime(2022, 3, 1, 7, 0, 0);
      _playSource.SimulatePlay("1", _clock.Now);
      await _sorter.NotifyChange(data.Select(x => x.preview), true);
      var result = _sorter.ResultLevels.value.ToList();
      _logger.Debug($"0: {result[0].levelID}, 1: {result[1].levelID}, 2: {result[2].levelID}");

      Assert.True(isChangedFired);
      IEnumerable<string> expectation = new List<int>() { 1, 0 }.Concat(Enumerable.Range(2, 998)).Select(i => $"{i}");
      Assert.Equal(expectation, _sorter.ResultLevels.value.Select(x => x.levelID));
    }

    private readonly IPALogger _logger;
    private readonly DiContainer _container;
    private readonly MockEventSource _playSource;
    private readonly InMemoryDateRepository _repository;
    private readonly LastPlayedDateSorter _sorter;
    private readonly FixedClock _clock;

    private IEnumerable<(MockPreview preview, DateTime date)> GenerateData() {
      return Enumerable.Range(0, 1000)
        .Select(i => (
          preview: new MockPreview($"{i}"),
          date: _clock.Now.AddSeconds(-Math.Pow(i, 3))));
    }
  }
}
