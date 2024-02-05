using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSort.Accuracy.External;
using BetterSort.Common.Compatibility;
using BetterSort.Common.External;
using BetterSort.Common.Interfaces;
using HarmonyLib;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Sorter {

  public class UIAwareSorter : ISorterCustom, ISorterWithLegend, ITransformerPlugin {
    private readonly FilterSortAdaptor _adaptor;
    private readonly AccuracySorter _sorter;
    private readonly SiraLog _logger;
    private readonly IBsInterop _bsInterop;
    private readonly ISongSelection _songSelection;
    private bool _isHooking;

    public UIAwareSorter(AccuracySorter sorter, SiraLog logger, IBsInterop bsInterop, FilterSortAdaptor adaptor, ISongSelection songSelection) {
      _adaptor = adaptor;
      _sorter = sorter;
      _logger = logger;
      _bsInterop = bsInterop;
      _songSelection = songSelection;

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
        _songSelection.OnSongSelected += SelectDifficultyWithGuard;
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

    private async void SelectDifficultyWithGuard(int index, LevelPreview preview) {
      try {
        await SelectDifficulty(index, preview).ConfigureAwait(false);
      }
      catch (DirectoryNotFoundException notFound) {
        _logger.Info($"Suppress IO exception, it may occur when delete beatmap; {notFound.Message}");
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }

    private async Task SelectDifficulty(int index, LevelPreview preview) {
      var type = Type.GetType("BetterSongList.HarmonyPatches.HookLevelCollectionTableSet, BetterSongList");
      if (type == null) {
        _logger.Warn($"Can't find current sorter while selecting difficulty. Skip.");
        return;
      }

      var sorter = AccessTools.StaticFieldRefAccess<ISorter>(type, "sorter");
      if (sorter != this) {
        _logger.Debug($"Not selecting this sort while selecting difficulty. {(_isHooking ? "Unhook" : "Skip")}.");
        if (_isHooking) {
          _songSelection.OnSongSelected -= SelectDifficultyWithGuard;
          _isHooking = false;
        }
        return;
      }

      if (index < 0 || _sorter.Mapping.Count <= index) {
        _logger.Debug($"User picked a song never played so skip selecting difficulty. ({index} {preview.LevelId} {preview.SongName})");
        return;
      }

      var record = _sorter.Mapping[index];
      string mode = record.Mode;

      _logger.Debug($"{nameof(SelectDifficulty)}: Set {index} {preview.SongName} {mode} {record.Difficulty} {record.Accuracy}");
      await _songSelection.SelectDifficulty(mode, record.Difficulty, preview);
    }
  }
}
