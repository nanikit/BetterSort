using BetterSongList.SortModels;
using BetterSort.Common.External;
using BetterSort.Common.Interfaces;
using System.Threading.Tasks;

namespace BetterSort.Common.Test.Mocks {

  internal class MockSongSelection : ISongSelection {

    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public ISorter? CurrentSorter => null;

    public Task SelectDifficulty(string TypeName, RecordDifficulty difficulty, LevelPreview preview) {
      return Task.CompletedTask;
    }
  }
}
