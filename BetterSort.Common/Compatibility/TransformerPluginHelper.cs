using BetterSongList;
using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using HarmonyLib;
using SiraUtil.Logging;
using System;

namespace BetterSort.Common.Compatibility {

  public interface ITransformerPluginHelper {

    void Register<T>(T plugin) where T : ITransformerPlugin, ISorterCustom;
  }

  public class TransformerPluginHelper(SiraLog logger) : ITransformerPluginHelper {
    private readonly SiraLog _logger = logger;

    public virtual void Register<T>(T plugin) where T : ITransformerPlugin, ISorterCustom {
      bool isRegistered = SortMethods.RegisterCustomSorter(plugin);
      if (isRegistered) {
        try {
          ForceUpdateDropdown();
          _logger.Info("Registered this to BetterSongList.");
        }
        catch (Exception exception) {
          _logger.Error(exception);
        }
      }
      else {
        _logger.Info("Failed to register sorter. Check AllowPluginSortsAndFilters config in BetterSongList.");
      }
    }

    // On 1.34.2 BetterSongList doesn't update dropdown when a new sorter is registered.
    // https://github.com/kinsi55/BeatSaber_BetterSongList/issues/29
    private static void ForceUpdateDropdown() {
      var type = AccessTools.TypeByName("BetterSongList.UI.FilterUI");
      object ui = AccessTools.StaticFieldRefAccess<object>(type, "persistentNuts");
      AccessTools.Method(ui.GetType(), "UpdateTransformerOptionsAndDropdowns").Invoke(ui, null);
    }
  }
}
