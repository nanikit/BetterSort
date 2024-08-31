using BetterSort.Common.External;
using BetterSort.Common.Models;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using HarmonyLib;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace BetterSort.Accuracy.External {
  public record PlayRecord(string LevelId, string Mode, RecordDifficulty Difficulty, double Accuracy);

  public interface IBsInterop {

    event Action<PlayRecord> OnSongPlayed;

    void SetPlaylistItem(IReadOnlyCollection<BaseBeatmapLevel> levels);
  }

  internal class BsUtilsInterop(SiraLog logger, Common.External.Scoresaber scoresaber, Beatleader beatleader) : IBsInterop, IInitializable, IDisposable {
    private static readonly MethodInfo _handleLevelCollectionViewControllerDidSelectLevel = AccessTools.Method(typeof(LevelCollectionNavigationController), "HandleLevelCollectionViewControllerDidSelectLevel");
    private bool _hasPlaylistManager = true;

    public event Action<PlayRecord> OnSongPlayed = delegate { };

    // It is needless as of https://github.com/rithik-b/PlaylistManager/commit/f6a1120cce0881e05fe92b9b5d4434badde7ac9c
    public void SetPlaylistItem(IReadOnlyCollection<BaseBeatmapLevel> levels) {
      try {
        if (!_hasPlaylistManager) {
          return;
        }
        var type = Type.GetType("PlaylistManager.HarmonyPatches.LevelCollectionViewController_SetData, PlaylistManager");
        if (type == null) {
          logger.Info($"{nameof(SetPlaylistItem)}: type is null, maybe there is no PlaylistManager mod.");
          _hasPlaylistManager = false;
          return;
        }
        string name = "beatmapLevels";
        var field = AccessTools.DeclaredField(type, name);
        if (field == null) {
          logger.Warn($"{nameof(SetPlaylistItem)}: field {name} is not found");
          _hasPlaylistManager = false;
          return;
        }
        logger.Debug($"{nameof(SetPlaylistItem)}: Try updating PlaylistManager levels.");
        AccessTools.StaticFieldRefAccess<IReadOnlyCollection<BaseBeatmapLevel>>(type, name) = levels;
      }
      catch (Exception ex) {
        logger.Error(ex);
      }
    }

    public void Initialize() {
      BSEvents.levelCleared += DispatchWithAccuracy;
    }

    public void Dispose() {
      BSEvents.levelCleared -= DispatchWithAccuracy;
    }

    private void DispatchWithAccuracy(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults result) {
      try {
        DispatchAccuracy(result);
      }
      catch (Exception ex) {
        logger.Error(ex);
      }
    }

    private void DispatchAccuracy(LevelCompletionResults result) {
      var setup = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
      if (setup == null) {
        logger.Warn($"Skip record as querying game stats is not possible.");
        return;
      }

#if NOT_BEFORE_1_36_2
      var level = setup.beatmapLevel;
      string? mode = setup.beatmapKey.beatmapCharacteristic.serializedName;
      string serializedDiff = setup.beatmapKey.difficulty.SerializedName();
#else
      var diffBeatmap = setup.difficultyBeatmap;
      var level = diffBeatmap?.level;
      string? mode = diffBeatmap?.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName;
      string? serializedDiff = diffBeatmap?.difficulty.SerializedName();
#endif

      string? levelId = level?.levelID;
      string? songName = level?.songName;
      var difficulty = RecordDifficultyExtension.ConvertFromString(serializedDiff);
      if (levelId == null || difficulty is not RecordDifficulty diff || mode == null) {
        logger.Warn($"Skip record as some game stats are missing: {levelId}, {mode}, {difficulty}, {songName}");
        return;
      }

      var transformed = setup.transformedBeatmapData;
      if (transformed == null) {
        string levelDescription = $"{levelId} {songName}";
        logger.Warn($"Skip record as accuracy is not available: {levelDescription}");
        return;
      }

      string? skipReason = GetSkipReason(setup.practiceSettings);
      if (skipReason != null) {
        logger.Info(skipReason);
        return;
      }

      int maxMultiplied = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformed);
      double accuracy = (double)result.multipliedScore / maxMultiplied;
      bool isFailed = setup.gameplayModifiers.noFailOn0Energy && result.energy == 0;
      double modifiedAccuracy = accuracy * (isFailed ? 0.5 : 1);

      logger.Debug($"Dispatch play event: {songName ?? "(null)"} {mode} {diff} {modifiedAccuracy}{(isFailed ? " NF" : "")}");
      OnSongPlayed?.Invoke(new PlayRecord(levelId, mode, diff, modifiedAccuracy));
    }

    private string? GetSkipReason(PracticeSettings practiceSettings) {
      if (scoresaber.IsInReplay()) {
        return "Skip scoresaber replay record.";
      }
      if (beatleader.IsInReplay()) {
        return "Skip beatleader replay record.";
      }

      if (practiceSettings != null) {
        return "Skip practice record.";
      }

      if (ScoreSubmission.Disabled) {
        return $"Skip record due to score submission being disabled by {ScoreSubmission.LastDisabledModString}.";
      }

      return null;
    }
  }
}
