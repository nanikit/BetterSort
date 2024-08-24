using BetterSongList.SortModels;
using BetterSort.Common.Models;
using HarmonyLib;
using IPA.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BetterSort.Common.External {

  public delegate void OnSongSelectedHandler(int index, LevelPreview preview);

  public interface ISongSelection {

    event OnSongSelectedHandler OnSongSelected;

    ISorter? CurrentSorter { get; }

    Task SelectDifficulty(string type, RecordDifficulty difficulty, LevelPreview preview);
  }

  public class SongSelection(SiraLog logger, PlayerDataModel playerData, StandardLevelDetailViewController levelDetailViewController) : ISongSelection, IAffinity {

    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public ISorter? CurrentSorter {
      get {
        var type = Type.GetType("BetterSongList.HarmonyPatches.HookLevelCollectionTableSet, BetterSongList");
        if (type == null) {
          logger.Warn($"Can't find current sorter while selecting difficulty. Skip.");
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

#if NOT_BEFORE_1_36_2

    [AffinityPrefix]
    [AffinityPatch(typeof(LevelCollectionNavigationController), "HandleLevelCollectionViewControllerDidSelectLevel")]
    protected void HandleLevelCollectionViewControllerDidSelectLevelPrefix(LevelCollectionViewController viewController, BaseBeatmapLevel level) {
      var view = viewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
      int row = view.GetField<int, LevelCollectionTableView>("_selectedRow");
      bool hasHeader = view.GetField<bool, LevelCollectionTableView>("_showLevelPackHeader");
      int index = hasHeader ? row - 1 : row;
      OnSongSelected.Invoke(index, new LevelPreview(level));
    }

#else
    [AffinityPostfix]
    [AffinityPatch(typeof(LevelCollectionNavigationController), "HandleLevelCollectionViewControllerDidSelectLevel")]
    protected void HandleLevelCollectionViewControllerDidSelectLevelPostfix(LevelCollectionViewController viewController, BaseBeatmapLevel level) {
      var view = viewController.GetField<LevelCollectionTableView, LevelCollectionViewController>("_levelCollectionTableView");
      int row = view.GetField<int, LevelCollectionTableView>("_selectedRow");
      bool hasHeader = view.GetField<bool, LevelCollectionTableView>("_showLevelPackHeader");
      int index = hasHeader ? row - 1 : row;
      OnSongSelected.Invoke(index, new LevelPreview(level));
    }
#endif

    private void SelectDifficultyInternal(string type, RecordDifficulty difficulty, LevelPreview preview) {
      logger.Debug($"Try selecting {preview.LevelId} {type} {difficulty}.");

      var diff = difficulty.ToGameDifficulty() ?? BeatmapDifficulty.ExpertPlus;

#if NOT_BEFORE_1_36_2

      var sameType = preview.Preview.GetCharacteristics().FirstOrDefault(x => x.serializedName == type);
      if (sameType == null) {
        var characteristics = preview.Preview.GetCharacteristics().Select(x => x.serializedName).ToList();
        logger.Warn($"BeatmapCharacteristic {type} not found in {string.Join(", ", characteristics)}. Quit.");
        return;
      }

      playerData.playerData.SetLastSelectedBeatmapCharacteristic(sameType);
      playerData.playerData.SetLastSelectedBeatmapDifficulty(diff);
      ReflectionUtil.InvokeMethod<object, StandardLevelDetailViewController>(levelDetailViewController, "ShowOwnedContent", []);
#else
      var view = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
      var viewLevel = view?.GetField<BaseBeatmapLevel, StandardLevelDetailView>("_level");
      if (viewLevel == null || preview == null) {
        logger.Warn("StandardLevelDetailView?._level is null. Quit.");
        return;
      }

      var sameType = preview.Preview.previewDifficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == type);
      if (sameType == null) {
        string characteristics = string.Join(", ", preview.Preview.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic.serializedName));
        logger.Warn($"BeatmapCharacteristic {type} not found in {string.Join(", ", characteristics)}. Quit.");
        return;
      }

      bool hasDifficulty = sameType.beatmapDifficulties.Contains(diff);
      if (!hasDifficulty) {
        logger.Warn($"BeatmapDifficulty {diff} not found in {string.Join(", ", sameType.beatmapDifficulties)}. Quit.");
        return;
      }

      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapCharacteristic), sameType.beatmapCharacteristic);
      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapDifficulty), diff);
      view.SetContent(viewLevel, diff, sameType.beatmapCharacteristic, player);
#endif
    }
  }
}
