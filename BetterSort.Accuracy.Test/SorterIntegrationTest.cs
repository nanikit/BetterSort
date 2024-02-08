using BetterSongList.SortModels;
using BetterSort.Accuracy.Installers;
using BetterSort.Accuracy.Sorter;
using BetterSort.Accuracy.Test.Mocks;
using BetterSort.Common.Interfaces;
using BetterSort.Common.Test;
using BetterSort.Common.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace BetterSort.Accuracy.Test {

  [TestClass]
  public class SorterIntegrationTest {
    private readonly ISorterCustom _adaptor;
    private readonly InMemoryRepository _repository;
    private readonly AccuracySorter _sorter;

    public SorterIntegrationTest() {
      var container = new DiContainer();
      container.Install<MockEnvironmentInstaller>();
      container.BindInterfacesAndSelfTo<InMemoryRepository>().AsSingle();
      container.BindInterfacesAndSelfTo<MockBsInterop>().AsSingle();
      container.Install<SorterInstaller>();

      _sorter = container.Resolve<AccuracySorter>();
      _adaptor = container.Resolve<ISorterCustom>();
      _repository = container.Resolve<InMemoryRepository>();
    }

    // BetterSongList can pass empty list.
    [TestMethod]
    public void TestEmptyCase() {
      var data = new List<IPreviewBeatmapLevel>().AsEnumerable();
      _adaptor.DoSort(ref data, true);
      CollectionAssert.AreEqual(new List<IPreviewBeatmapLevel>(), data.ToList());
    }

    [TestMethod]
    public async Task TestComplexCase() {
      _repository.BestAccuracies = new() {
        { "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", new() {
            { "Standard", new() {
              { Common.External.RecordDifficulty.ExpertPlus, 0.90292 }
            }},
        }},
      };
      var input = new List<MockPreview>() {
        new("custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"),
        new("custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
        new("custom_level_cccccccccccccccccccccccccccccccccccccccc"),
      };
      var result = await WaitResult(input, isSelected: true).ConfigureAwait(false);
      CollectionAssert.AreEqual(result.Levels.Select(x => x.LevelId).ToList(), new List<string> {
        "custom_level_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        "custom_level_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
        "custom_level_cccccccccccccccccccccccccccccccccccccccc",
      });
      CollectionAssert.AreEqual(result.Legend.ToList(), new List<(string Label, int Index)>() { ("90.29", 0), ("N/A", 1) });
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
