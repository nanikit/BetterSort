namespace BetterSort.Accuracy.External
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BeatmapDifficulty
    {
        Easy = 1,
        Normal = 3,
        Hard = 5,
        Expert = 7,
        ExpertPlus = 9,
    }

    public class BestRecord
    {
        public string SongHash { get; set; } = "";

        public string Mode { get; set; } = "";

        public BeatmapDifficulty Difficulty { get; set; }

        public int Score { get; set; }

        public double Accuracy { get; set; }
    }
}

