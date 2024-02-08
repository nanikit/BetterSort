using BetterSongList.SortModels;
using BetterSort.Common.Models;
using HarmonyLib;
using IPA.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterSort.Common.External {

  public delegate void OnSongSelectedHandler(int index, LevelPreview preview);

  public interface ISongSelection {

    event OnSongSelectedHandler OnSongSelected;

    ISorter? CurrentSorter { get; }

    Task SelectDifficulty(string type, RecordDifficulty difficulty, LevelPreview preview);
  }

  public class SongSelection(SiraLog logger) : ISongSelection, IAffinity {
    private readonly SiraLog _logger = logger;

    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public ISorter? CurrentSorter {
      get {
        var type = Type.GetType("BetterSongList.HarmonyPatches.HookLevelCollectionTableSet, BetterSongList");
        if (type == null) {
          _logger.Warn($"Can't find current sorter while selecting difficulty. Skip.");
          return null;
        }

        return AccessTools.StaticFieldRefAccess<ISorter>(type, "sorter");
      }
    }

    public async Task SelectDifficulty(string type, RecordDifficulty difficulty, LevelPreview preview) {
      await UnityMainThreadTaskScheduler.Factory.StartNew(
        () => SelectDifficultyInternal(type, difficulty, preview)
      ).ConfigureAwait(false);
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(LevelCollectionNavigationController), "HandleLevelCollectionViewControllerDidSelectLevel")]
    protected void HandleLevelCollectionViewControllerDidSelectLevelPostfix(LevelCollectionViewController viewController, IPreviewBeatmapLevel level) {
      var view = viewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
      int row = view.GetField<int, LevelCollectionTableView>("_selectedRow");
      bool hasHeader = view.GetField<bool, LevelCollectionTableView>("_showLevelPackHeader");
      int index = hasHeader ? row - 1 : row;
      OnSongSelected.Invoke(index, new LevelPreview(level));
    }

    private void SelectDifficultyInternal(string type, RecordDifficulty difficulty, LevelPreview preview) {
      var player = UnityEngine.Object.FindObjectOfType<PlayerDataModel>()?.playerData;
      if (player == null) {
        _logger.Warn("playerData is null. Quit.");
        return;
      }

      var view = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
      var viewLevel = view?.GetField<IBeatmapLevel, StandardLevelDetailView>("_level");
      if (view == null || preview == null) {
        _logger.Warn("StandardLevelDetailView?._level is null. Quit.");
        return;
      }

      var sameType = preview.Preview.previewDifficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == type);
      if (sameType == null) {
        string characteristics = string.Join(", ", preview.Preview.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic.serializedName));
        _logger.Warn($"BeatmapCharacteristic {type} not found in {string.Join(", ", characteristics)}. Quit.");
        return;
      }

      var diff = difficulty.ToGameDifficulty() ?? BeatmapDifficulty.ExpertPlus;
      bool hasDifficulty = sameType.beatmapDifficulties.Contains(diff);
      if (!hasDifficulty) {
        _logger.Warn($"BeatmapDifficulty {diff} not found in {string.Join(", ", sameType.beatmapDifficulties)}. Quit.");
        return;
      }

      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapCharacteristic), sameType.beatmapCharacteristic);
      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapDifficulty), diff);
      view.SetContent(viewLevel, diff, sameType.beatmapCharacteristic, player);
    }
  }
}
