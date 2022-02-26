namespace BetterSongList.LastPlayedSort.Test {
  using BetterSongList.LastPlayedSort.External;
  using Nanikit.Test;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Xunit;
  using IPALogger = IPA.Logging.Logger;

  internal class SorterTest {
    public SorterTest(IPALogger logger) {
      _logger = logger;
    }

    [Test]
    public async Task TestOneshotSort() {
      var now = new DateTime(2022, 3, 1);
      var sorter = new LastPlayedDateSorter(new FixedClock(now));
      bool isChangedFired = false;
      sorter.ResultLevels.didChangeEvent += () => {
        isChangedFired = true;
      };

      var data = Enumerable.Range(0, 1000)
        .Select(i => (
          preview: new MockPreview($"{i}"),
          date: now.AddSeconds(-Math.Pow(i, 3))))
        .ToList();
      sorter.LastPlayedDates = data.ToDictionary(x => x.preview.levelID, x => x.date);

      data.ShuffleInPlace();
      await sorter.NotifyChange(data.Select(x => x.preview), true);

      var legend = sorter.Legend.value.ToList();

      Assert.True(isChangedFired, "ResultLevels.didChangeEvent is not fired.");
      Assert.Equal(Enumerable.Range(0, 1000).Select(i => $"{i}"), sorter.ResultLevels.value.Select(x => x.levelID));
      Assert.Equal("Yesterday", legend[0].Label);
      Assert.Equal(0, legend[0].Index);
      Assert.Equal("This week", legend[1].Label);
      Assert.Equal(45, legend[1].Index);
      Assert.Equal("6 years ago", legend[10].Label);
      Assert.Equal(456, legend[10].Index);
    }

    private readonly IPALogger _logger;
  }

  class FixedClock : IClock {
    public FixedClock(DateTime now) {
      Now = now;
    }

    public DateTime Now { get; private set; }
  }
}
