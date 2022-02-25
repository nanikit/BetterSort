namespace BetterSongList.LastPlayedSort.Test {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Nanikit.Test;
  using Xunit;
  using IPALogger = IPA.Logging.Logger;

  internal class SorterTest {
    public SorterTest(IPALogger logger) {
      _logger = logger;
    }

    //[Test]
    public void Test() {
      var now = DateTime.Parse("2022-02-24 00:00:00");

      //Console.WriteLine(FormatRelativeTime(-9));
      //Console.WriteLine(FormatRelativeTime(0));

      //var nowUnix = now.ToUnixTime();
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 23:59:59").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 23:57:59").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 23:55:59").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 22:00:00").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 19:00:00").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-23 03:00:00").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-22 03:00:00").ToUnixTime()));
      //Console.WriteLine(FormatRelativeTime(nowUnix - DateTime.Parse("2022-02-11 03:00:00").ToUnixTime()));
      var levels = new List<MockPreview>();
      var lastPlays = new Dictionary<string, DateTime>();
      var date = DateTime.Now;
      for (int i = 0; i < 1000; i++) {
        var level = new MockPreview($"{i}");
        levels.Add(level);
        lastPlays[level.levelID] = date;

        date = (date.ToUnixTime() - i * i).AsUnixTime();
        _logger.Info($"id{i}: {date}");
      }

      var result = LastPlayedDateSorter.GetLegendByLogScale(levels.ToArray(), now, lastPlays);
      foreach (var legend in result) {
        _logger.Info($"{legend}");
      }
    }

    [Test]
    public async Task TestOneshotSort() {
      var sorter = new LastPlayedDateSorter();
      bool isChangedFired = false;
      sorter.ResultLevels.didChangeEvent += () => {
        isChangedFired = true;
      };

      var data = Enumerable.Range(0, 100).Select(i => (new MockPreview($"{i}"), new DateTime())).ToList();
      sorter.LastPlayedDates = data.ToDictionary(x => x.Item1.levelID, x => x.Item2);

      data.ShuffleInPlace();
      await sorter.NotifyChange(data.Select(x => x.Item1), true);

      foreach (var legend in sorter.Legend.value) {
        _logger.Info($"{legend}");
      }

      Assert.True(isChangedFired, "ResultLevels.didChangeEvent is not fired.");
      Assert.Equal(Enumerable.Range(0, 100).Select(i => $"{i}"), sorter.ResultLevels.value.Select(x => x.levelID));
    }

    private readonly IPALogger _logger;

  }
}
