using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSort.Common.Compatibility;

namespace BetterSort.Test.Common.Mocks {

  public class MockTransformerPluginHelper : ITransformerPluginHelper {

    public void Register<T>(T plugin) where T : ITransformerPlugin, ISorterCustom {
      // Do nothing.
    }
  }
}
