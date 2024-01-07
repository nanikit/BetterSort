using BetterSongList.SortModels;

using BetterSort.Accuracy.Sorter;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Interfaces;
using BetterSort.Test.Common.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Test {

  public class SorterTest {
    private readonly IPALogger _logger;
    private readonly DiContainer _container;
    private readonly ISorterCustom _adaptor;
    private readonly InMemoryRepository _repository;
    private readonly AccuracySorter _sorter;

    public SorterTest(ITestOutputHelper output) {
      _logger = new MockLogger(output);

      var container = new DiContainer();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(_logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<MockBsInterop>().AsSingle().WhenInjectedInto<UIAwareSorter>();

      container.Install<AccuracyInstaller>();
      _container = container;

      _sorter = container.Resolve<AccuracySorter>();
      _adaptor = container.Resolve<ISorterCustom>();
      _repository = container.Resolve<InMemoryRepository>();
    }

    // BetterSongList can pass empty list.
    [Fact]
    public void TestEmptySort() {
      var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      _adaptor.DoSort(ref data, true);
      Assert.Empty(data);
    }

    [Fact]
    public async Task TestSort() {
      _repository.BestAccuracies = new() {
        { "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", new() {
            { "Standard", new() {
              { RecordDifficulty.ExpertPlus, 0.90292 }
            }},
        }},
      };
      var input = new List<MockPreview>() {
        new MockPreview("custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"),
        new MockPreview("custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
        new MockPreview("custom_level_cccccccccccccccccccccccccccccccccccccccc"),
      };
      var result = await WaitResult(input, isSelected: true).ConfigureAwait(false);
      Assert.Equal(result.Levels.Select(x => x.LevelId), new List<string> {
        "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        "custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
        "custom_level_cccccccccccccccccccccccccccccccccccccccc",
      });
      Assert.Equal(result.Legend!, new List<(string Label, int Index)>() { ("90.29", 0), ("N/A", 1) });
    }

    private async Task<ISortFilterResult> WaitResult(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false, CancellationToken? token = null) {
      TaskCompletionSource<ISortFilterResult?> completer = new();
      void SetResult(ISortFilterResult? res) {
        completer.TrySetResult(res);
      }
      _sorter.OnResultChanged += SetResult;

      _sorter.NotifyChange(newLevels, isSelected, token);

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
