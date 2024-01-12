using BetterSongList.SortModels;
using BetterSort.Accuracy.Sorter;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Interfaces;
using BetterSort.Test.Common.Mocks;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zenject;

namespace BetterSort.Accuracy.Test {

  public class SorterIntegrationTest {
    private readonly SiraLog _logger;
    private readonly ISorterCustom _adaptor;
    private readonly InMemoryRepository _repository;
    private readonly AccuracySorter _sorter;

    public SorterIntegrationTest(ITestOutputHelper output) {
      _logger = new MockLogger(output);

      var container = new DiContainer();
      container.BindInterfacesAndSelfTo<SiraLog>().FromInstance(_logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<InMemoryRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<MockBsInterop>().AsSingle().WhenInjectedInto<UIAwareSorter>();

      container.Install<AccuracyInstaller>();

      _sorter = container.Resolve<AccuracySorter>();
      _adaptor = container.Resolve<ISorterCustom>();
      _repository = container.Resolve<InMemoryRepository>();
    }

    // BetterSongList can pass empty list.
    [Fact]
    public void TestEmptyCase() {
      var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      _adaptor.DoSort(ref data, true);
      Assert.Empty(data);
    }

    [Fact]
    public async Task TestComplexCase() {
      _repository.BestAccuracies = new() {
        { "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", new() {
            { "Standard", new() {
              { RecordDifficulty.ExpertPlus, 0.90292 }
            }},
        }},
      };
      var input = new List<MockPreview>() {
        new("custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"),
        new("custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
        new("custom_level_cccccccccccccccccccccccccccccccccccccccc"),
      };
      var result = await WaitResult(input, isSelected: true).ConfigureAwait(false);
      Assert.Equal(result.Levels.Select(x => x.LevelId), new List<string> {
        "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        "custom_level_cccccccccccccccccccccccccccccccccccccccc",
        "custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
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
