#if NOT_BEFORE_1_36_2

global using BaseBeatmapLevel = BeatmapLevel;
global using BaseLevelPack = BeatmapLevelPack;

#else
global using BaseBeatmapLevel = IPreviewBeatmapLevel;
global using BaseLevelPack = IAnnotatedBeatmapLevelCollection;
#endif
