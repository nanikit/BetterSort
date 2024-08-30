using BetterSort.Accuracy.External;
using BetterSort.Accuracy.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BetterSort.Accuracy.Test {

  [TestClass]
  public class ImportTest {

    [TestMethod]
    public void TestBeatLeader() {
      var beatLeader = new BeatleaderBest();
      string url = beatLeader.GetRecordUrl(MockId.QuitUserId, 1);

      Assert.AreEqual(url, $"https://api.beatleader.xyz/player/{MockId.QuitUserId}/scores?page=1&sortBy=date&order=desc");

      string json = File.ReadAllText(@"..\..\Data\beatleader_scores.json");
      var (Records, Paging, Log) = beatLeader.ToBestRecords(json);

      Assert.AreEqual(8, Records.Count);
      Assert.IsTrue(0 < Records[0].Accuracy && Records[0].Accuracy <= 1);
      Assert.AreEqual(1849, Paging.Total);
      Assert.IsNull(Log);

      // NF modifier multiplier should be 0.5
      Assert.AreEqual(0.754181 / 2, Records[1].Accuracy, 0.00001);
    }

    [TestMethod]
    public void TestScoresaber() {
      var scoresaber = new ScoresaberBest();
      string url = scoresaber.GetRecordUrl(MockId.QuitUserId, 1);

      Assert.AreEqual(url, $"https://scoresaber.com/api/player/{MockId.QuitUserId}/scores?page=1&sort=recent");

      string json = File.ReadAllText(@"..\..\Data\scoresaber_scores.json");
      var (Records, Paging, Log) = scoresaber.ToBestRecords(json);

      Assert.AreEqual(8, Records.Count);
      Assert.IsTrue(0 < Records[0].Accuracy && Records[0].Accuracy <= 1);
      Assert.AreEqual(2133, Paging.Total);
      Assert.IsNull(Log);

      // NF modifier multiplier should be 0.5
      Assert.AreEqual(0.754181 / 2, Records[1].Accuracy, 0.00001);
    }
  }
}
