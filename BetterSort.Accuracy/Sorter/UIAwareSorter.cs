using BetterSongList.HarmonyPatches;
using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using BetterSort.Common.Interfaces;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.Sorter {
  public class UIAwareSorter : ISorterCustom, ISorterWithLegend, ITransformerPlugin {
    private readonly FilterSortAdaptor _adaptor;
    private readonly AccuracySorter _sorter;
    private readonly IPALogger _logger;
    private readonly IBsInterop _bsInterop;
    private bool _isHooking;

    public UIAwareSorter(AccuracySorter sorter, IPALogger logger, IBsInterop bsInterop) {
      _adaptor = new FilterSortAdaptor(sorter, logger);
      _sorter = sorter;
      _logger = logger;
      _bsInterop = bsInterop;

      _sorter.OnResultChanged += UpdatePlaylistManager;
    }

    public bool isReady => _adaptor.isReady;

    public string name => _adaptor.name;

    public bool visible => _adaptor.visible;

    public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
      return _adaptor.BuildLegend(levels);
    }

    public void ContextSwitch(SelectLevelCategoryViewController.LevelCategory levelCategory, IAnnotatedBeatmapLevelCollection? playlist) {
      _adaptor.ContextSwitch(levelCategory, playlist);
    }

    public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels, bool ascending) {
      _adaptor.DoSort(ref levels, ascending);
      if (!_isHooking) {
        _isHooking = true;
        _bsInterop.OnSongSelected += SelectDifficultyWithGuard;
      }
    }

    public Task Prepare(CancellationToken cancelToken) {
      return _adaptor.Prepare(cancelToken);
    }

    private void UpdatePlaylistManager(ISortFilterResult? result) {
      if (result != null) {
        _bsInterop.SetPlaylistItem(result.Levels.OfType<LevelPreview>().Select(x => x.Preview).ToList());
      }
    }

    private async void SelectDifficultyWithGuard(int index, IPreviewBeatmapLevel level) {
      try {
        await SelectDifficulty(index, level).ConfigureAwait(false);
      }
      catch (DirectoryNotFoundException notFound) {
        _logger.Info($"Suppress IO exception, it may occur when delete beatmap; {notFound.Message}");
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task SelectDifficulty(int index, IPreviewBeatmapLevel level) {
      var type = Type.GetType("BetterSongList.HarmonyPatches.HookLevelCollectionTableSet, BetterSongList");
      if (type == null) {
        _logger.Warn($"{nameof(SelectDifficulty)}: Can't find current sorter. Skip.");
      }
      var sorter = AccessTools.StaticFieldRefAccess<ISorter>(type, "sorter");
      if (sorter != this) {
        _logger.Debug($"{nameof(SelectDifficulty)}: Not selected this sort. Skip.");
        if (_isHooking) {
          _bsInterop.OnSongSelected -= SelectDifficultyWithGuard;
          _isHooking = false;
        }
        return;
      }

      if (index < 0 || _sorter.Mapping.Count <= index) {
        _logger.Debug($"{nameof(SelectDifficulty)}: Not played level. Skip. ({index} {level.levelID} {level.songName})");
        return;
      }

      var record = _sorter.Mapping[index];
      string mode = record.Mode;
      var characteristic = level.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).FirstOrDefault(x => x.serializedName == mode);
      if (characteristic == null) {
        _logger.Warn($"{nameof(SelectDifficulty)}: Matching characteristic is not found. Quit. ({level.levelID}, {mode})");
        return;
      }

      _logger.Debug($"{nameof(SelectDifficulty)}: Set {index} {level.songName} {mode} {record.Difficulty} {record.Accuracy}");
      await _bsInterop.SetModeAndDifficulty(characteristic, record.Difficulty).ConfigureAwait(false);
    }

  }
}

