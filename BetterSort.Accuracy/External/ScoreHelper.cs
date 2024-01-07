using BetterSort.Accuracy.Sorter;
using IPA.Utilities.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Accuracy.External {

  internal class ScoreHelper {
    private readonly IPALogger _logger;

    public ScoreHelper(IPALogger logger) {
      _logger = logger;
    }

    public async Task<int?> CalculateMaxScore(IDifficultyBeatmap beatmap) {
      var setup = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
      if (setup == null) {
        _logger.Info($"Setup data is null, give up calculating maxScore: {beatmap.level.songName}");
        return null;
      }

      var data = await beatmap.GetBeatmapDataAsync(setup.environmentInfo, setup.playerSpecificSettings).ConfigureAwait(false);
      if (data == null) {
        _logger.Info($"Beatmap data is null, give up calculating maxScore: {beatmap.level.songName}");
        return null;
      }

      return ScoreModel.ComputeMaxMultipliedScoreForBeatmap(data);
    }

    public async Task<IDifficultyBeatmap?> GetDifficultyBeatmap(string levelId, string characteristic, RecordDifficulty difficulty) {
      var model = await UnityMainThreadTaskScheduler.Factory.StartNew(
        () => Object.FindObjectOfType<BeatmapLevelsModel>()
      ).ConfigureAwait(false);
      if (model?.customLevelPackCollection?.beatmapLevelPacks is not IBeatmapLevelPack[] packs) {
        _logger.Info($"Level pack is not found. Give up finding difficulty: {levelId} {characteristic} {difficulty}");
        return null;
      }

      //// custom_levelpack_CustomLevels
      string customLevelPackId = $"{CustomLevelLoader.kCustomLevelPackPrefixId}CustomLevels";
      var pack = packs.FirstOrDefault(x => x.packID == customLevelPackId);
      if (pack?.beatmapLevelCollection?.beatmapLevels is not IReadOnlyList<IPreviewBeatmapLevel> previews) {
        _logger.Info($"Custom level pack is not found. Give up finding difficulty: {levelId} {characteristic} {difficulty}");
        return null;
      }

      var preview = previews.FirstOrDefault(x => x.levelID == levelId);
      var mapSet = preview?.previewDifficultyBeatmapSets?.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == characteristic);
      var characteristicSO = mapSet?.beatmapCharacteristic;
      var gameDiff = difficulty.ToGameDifficulty();
      if (characteristicSO == null || gameDiff is not global::BeatmapDifficulty diff) {
        _logger.Info($"Level difficulty is not found. Give up finding difficulty: {levelId} {characteristic} {difficulty}");
        return null;
      }

      var result = await model.GetBeatmapLevelAsync(levelId, default);
      var ret = result.beatmapLevel?.beatmapLevelData?.GetDifficultyBeatmap(mapSet?.beatmapCharacteristic, diff);
      if (ret == null) {
        _logger.Info($"Failed to getting data. Give up finding difficulty: {levelId} {characteristic} {difficulty}");
      }
      return ret;
    }
  }
}
