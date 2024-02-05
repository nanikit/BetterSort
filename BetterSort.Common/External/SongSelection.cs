using BetterSort.Common.Interfaces;
using IPA.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterSort.Common.External {

  public delegate void OnSongSelectedHandler(int index, LevelPreview preview);

  [JsonConverter(typeof(JsonStringEnumConverter<RecordDifficulty>))]
  public enum RecordDifficulty {
    Easy = 1,
    Normal = 3,
    Hard = 5,
    Expert = 7,
    ExpertPlus = 9,
  }

  public interface ISongSelection {

    event OnSongSelectedHandler OnSongSelected;

    Task SelectDifficulty(string TypeName, RecordDifficulty difficulty, LevelPreview preview);
  }

  public static class RecordDifficultyExtension {

    public static RecordDifficulty? ConvertFromString(string? beatleaderDifficulty) {
      return beatleaderDifficulty switch {
        "Easy" => RecordDifficulty.Easy,
        "Normal" => RecordDifficulty.Normal,
        "Hard" => RecordDifficulty.Hard,
        "Expert" => RecordDifficulty.Expert,
        "ExpertPlus" => RecordDifficulty.ExpertPlus,
        _ => null,
      };
    }

    internal static BeatmapDifficulty? ToGameDifficulty(this RecordDifficulty difficulty) {
      return difficulty switch {
        RecordDifficulty.Easy => BeatmapDifficulty.Easy,
        RecordDifficulty.Normal => BeatmapDifficulty.Normal,
        RecordDifficulty.Hard => BeatmapDifficulty.Hard,
        RecordDifficulty.Expert => BeatmapDifficulty.Expert,
        RecordDifficulty.ExpertPlus => BeatmapDifficulty.ExpertPlus,
        _ => null,
      };
    }
  }

  public class SongSelection : ISongSelection, IAffinity {
    private readonly SiraLog _logger;

    public SongSelection(SiraLog logger) {
      _logger = logger;
    }

    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public async Task SelectDifficulty(string TypeName, RecordDifficulty difficulty, LevelPreview preview) {
      await UnityMainThreadTaskScheduler.Factory.StartNew(
        () => SelectDifficultyInternal(TypeName, difficulty, preview)
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

    private void SelectDifficultyInternal(string TypeName, RecordDifficulty difficulty, LevelPreview preview) {
      var player = Object.FindObjectOfType<PlayerDataModel>()?.playerData;
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

      var type = preview.Preview.previewDifficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == TypeName);
      if (type == null) {
        string characteristics = string.Join(", ", preview.Preview.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic.serializedName));
        _logger.Warn($"BeatmapCharacteristic {TypeName} not found in {string.Join(", ", characteristics)}. Quit.");
        return;
      }

      var diff = difficulty.ToGameDifficulty() ?? BeatmapDifficulty.ExpertPlus;
      bool hasDifficulty = type.beatmapDifficulties.Contains(diff);
      if (!hasDifficulty) {
        _logger.Warn($"BeatmapDifficulty {diff} not found in {string.Join(", ", type.beatmapDifficulties)}. Quit.");
        return;
      }

      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapCharacteristic), type.beatmapCharacteristic);
      player.SetProperty(nameof(PlayerData.lastSelectedBeatmapDifficulty), diff);
      view.SetContent(viewLevel, diff, type.beatmapCharacteristic, player);
    }
  }
}
