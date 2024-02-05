using BetterSort.Common.External;
using BetterSort.Common.Interfaces;
using System.Threading.Tasks;

namespace BetterSort.Test.Common.Mocks {

  internal class MockSongSelection : ISongSelection {

    public event OnSongSelectedHandler OnSongSelected = delegate { };

    public Task SelectDifficulty(string TypeName, RecordDifficulty difficulty, LevelPreview preview) {
      return Task.CompletedTask;
    }
  }
}
