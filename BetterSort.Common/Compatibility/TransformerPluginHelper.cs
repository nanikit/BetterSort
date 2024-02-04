using BetterSongList;
using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSongList.UI;
using HarmonyLib;
using SiraUtil.Logging;

namespace BetterSort.Common.Compatibility {

  public class TransformerPluginHelper {
    private readonly SiraLog _logger;

    public TransformerPluginHelper(SiraLog logger) {
      _logger = logger;
    }

    public void Register<T>(T plugin) where T : ITransformerPlugin, ISorterCustom {
      bool isRegistered = SortMethods.RegisterCustomSorter(plugin);
      if (isRegistered) {
        var ui = AccessTools.StaticFieldRefAccess<FilterUI>(typeof(FilterUI), "persistentNuts");
        AccessTools.Method(ui.GetType(), "UpdateTransformerOptionsAndDropdowns").Invoke(ui, null);
        _logger.Info("Registered this to BetterSongList.");
      }
      else {
        _logger.Info("Failed to register sorter. Check AllowPluginSortsAndFilters config in BetterSongList.");
      }
    }
  }
}
