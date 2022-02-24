using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.LastPlayedSort.Test {
  internal class SorterTest {

    public static void Test() {
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

      var levels = new List<MockPreview>() {
        new MockPreview("0.497497209502748"),
        new MockPreview("0.158270648884619"),
        new MockPreview("0.912065139133497"),
        new MockPreview("0.866339084094688"),
        new MockPreview("0.564701326498734"),
        new MockPreview("0.566918430275677"),
        new MockPreview("0.0522463097615911"),
        new MockPreview("0.213878956428028"),
        new MockPreview("0.633394994145263"),
        new MockPreview("0.604960703103224"),
        new MockPreview("0.939412308606087"),
        new MockPreview("0.0603125528311815"),
        new MockPreview("0.755130122753981"),
        new MockPreview("0.096149089429252"),
        new MockPreview("0.358672578901029"),
        new MockPreview("0.231518316488539"),
        new MockPreview("0.372667233476893"),
        new MockPreview("0.675516466077023"),
        new MockPreview("0.0527415191283621"),
        new MockPreview("0.999239611050409"),
        new MockPreview("0.395369311715164"),
        new MockPreview("0.615891615428866"),
        new MockPreview("0.00211766760135956"),
        new MockPreview("0.787501408355308"),
        new MockPreview("0.0934189561720827"),
        new MockPreview("0.763732516825844"),
        new MockPreview("0.962093610871059"),
        new MockPreview("0.998033316532786"),
        new MockPreview("0.280464602352062"),
        new MockPreview("0.266285633794427"),
        new MockPreview("0.774463753491092"),
        new MockPreview("0.479478278492155"),
        new MockPreview("0.0787919200330317"),
        new MockPreview("0.401475042580808"),
        new MockPreview("0.270929452902411"),
        new MockPreview("0.272845412531059"),
        new MockPreview("0.624746142705038"),
        new MockPreview("0.746590035576852"),
        new MockPreview("0.942838169957132"),
        new MockPreview("0.238935526706907"),
        new MockPreview("0.393983085556174"),
        new MockPreview("0.24174843844408"),
        new MockPreview("0.559679284030164"),
        new MockPreview("0.135422847559176"),
        new MockPreview("0.780643443859708"),
        new MockPreview("0.849896513229845"),
        new MockPreview("0.586857754663187"),
        new MockPreview("0.782538537019287"),
        new MockPreview("0.0398393687358253"),
        new MockPreview("0.135918060988172"),
        new MockPreview("0.0901225768268044"),
        new MockPreview("0.941274462159468"),
        new MockPreview("0.656231908958423"),
        new MockPreview("0.296674156939962"),
        new MockPreview("0.988626255677215"),
        new MockPreview("0.730872264367889"),
        new MockPreview("0.926980748984549"),
        new MockPreview("0.146201957827366"),
        new MockPreview("0.914105010095618"),

      };

      var result = LastPlayedDateSorter.GetLegendByLogScale(levels.ToArray(), now, new Dictionary<string, DateTime>() {
  { levels[0].levelID, DateTime.Parse("2022-02-24 00:00:00") },
  { levels[1].levelID, DateTime.Parse("2022-02-23 23:57:00") },
  { levels[2].levelID, DateTime.Parse("2022-02-23 23:54:00") },
  { levels[3].levelID, DateTime.Parse("2022-02-23 23:51:00") },
  { levels[4].levelID, DateTime.Parse("2022-02-23 23:48:00") },
  { levels[5].levelID, DateTime.Parse("2022-02-23 23:45:00") },
  { levels[6].levelID, DateTime.Parse("2022-02-23 23:42:00") },
  { levels[7].levelID, DateTime.Parse("2022-02-23 23:39:00") },
  { levels[8].levelID, DateTime.Parse("2022-02-23 23:36:00") },
  { levels[9].levelID, DateTime.Parse("2022-02-23 23:33:00") },
  { levels[10].levelID, DateTime.Parse("2022-02-23 23:30:00") },
  { levels[11].levelID, DateTime.Parse("2022-02-23 23:27:00") },
  { levels[12].levelID, DateTime.Parse("2022-02-23 23:24:00") },
  { levels[13].levelID, DateTime.Parse("2022-02-23 23:21:00") },
  { levels[14].levelID, DateTime.Parse("2022-02-23 23:18:00") },
  { levels[15].levelID, DateTime.Parse("2022-02-23 23:15:00") },
  { levels[16].levelID, DateTime.Parse("2022-02-23 23:12:00") },
  { levels[17].levelID, DateTime.Parse("2022-02-23 23:09:00") },
  { levels[18].levelID, DateTime.Parse("2022-02-23 23:06:00") },
  { levels[19].levelID, DateTime.Parse("2022-02-23 23:03:00") },
  { levels[20].levelID, DateTime.Parse("2022-02-23 23:00:00") },
  { levels[21].levelID, DateTime.Parse("2022-02-23 22:00:00") },
  { levels[22].levelID, DateTime.Parse("2022-02-23 21:00:00") },
  { levels[23].levelID, DateTime.Parse("2022-02-23 20:00:00") },
  { levels[24].levelID, DateTime.Parse("2022-02-23 19:00:00") },
  { levels[25].levelID, DateTime.Parse("2022-02-23 18:00:00") },
  { levels[26].levelID, DateTime.Parse("2022-02-23 17:00:00") },
  { levels[27].levelID, DateTime.Parse("2022-02-23 16:00:00") },
  { levels[28].levelID, DateTime.Parse("2022-02-23 15:00:00") },
  { levels[29].levelID, DateTime.Parse("2022-02-23 14:00:00") },
  { levels[30].levelID, DateTime.Parse("2022-02-23 13:00:00") },
  { levels[31].levelID, DateTime.Parse("2022-02-23 12:00:00") },
  { levels[32].levelID, DateTime.Parse("2022-02-23 11:00:00") },
  { levels[33].levelID, DateTime.Parse("2022-02-23 10:00:00") },
  { levels[34].levelID, DateTime.Parse("2022-02-23 09:00:00") },
  { levels[35].levelID, DateTime.Parse("2022-02-23 08:00:00") },
  { levels[36].levelID, DateTime.Parse("2022-02-23 07:00:00") },
  { levels[37].levelID, DateTime.Parse("2022-02-23 06:00:00") },
  { levels[38].levelID, DateTime.Parse("2022-02-22 06:00:00") },
  { levels[39].levelID, DateTime.Parse("2022-02-21 06:00:00") },
  { levels[40].levelID, DateTime.Parse("2022-02-20 06:00:00") },
  { levels[41].levelID, DateTime.Parse("2022-02-19 06:00:00") },
  { levels[42].levelID, DateTime.Parse("2022-02-18 06:00:00") },
  { levels[43].levelID, DateTime.Parse("2022-02-17 06:00:00") },
  { levels[44].levelID, DateTime.Parse("2022-02-16 06:00:01") },
  { levels[45].levelID, DateTime.Parse("2022-02-15 06:00:01") },
  { levels[46].levelID, DateTime.Parse("2022-02-14 06:00:01") },
  { levels[47].levelID, DateTime.Parse("2022-02-13 06:00:01") },
  { levels[48].levelID, DateTime.Parse("2022-02-12 06:00:01") },
  { levels[49].levelID, DateTime.Parse("2022-02-11 06:00:01") },
  { levels[50].levelID, DateTime.Parse("2022-02-10 06:00:01") },
  { levels[51].levelID, DateTime.Parse("2022-02-09 06:00:01") },
  { levels[52].levelID, DateTime.Parse("2022-02-08 06:00:01") },
  { levels[53].levelID, DateTime.Parse("2022-02-07 06:00:01") },
  { levels[54].levelID, DateTime.Parse("2022-02-06 06:00:01") },
  { levels[55].levelID, DateTime.Parse("2022-02-05 06:00:02") },
  { levels[56].levelID, DateTime.Parse("2022-02-04 06:00:02") },
  { levels[57].levelID, DateTime.Parse("2022-02-03 06:00:02") },
  { levels[58].levelID, DateTime.Parse("2022-02-02 06:00:02") },
      });
      foreach (var legend in result) {
        Console.WriteLine(legend);
      }
    }
  }
}
