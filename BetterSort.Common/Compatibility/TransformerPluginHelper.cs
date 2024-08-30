using BetterSongList;
using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using HarmonyLib;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSort.Common.Compatibility {

  public interface ITransformerPluginHelper {

    void Register(FilterSortAdaptor plugin);
  }

  public class TransformerPluginHelper(SiraLog logger) : ITransformerPluginHelper {
    private static Dictionary<string, FilterSortReference> _registeredSorters = [];

    public virtual void Register(FilterSortAdaptor plugin) {
      if (_registeredSorters.TryGetValue(plugin.name, out FilterSortReference stable)) {
        logger.Debug($"Already registered {plugin.name}. Replace.");
        stable.SetSorter(plugin);
        return;
      }

      var sorter = new FilterSortReference(plugin);
      bool isRegistered = SortMethods.RegisterCustomSorter(sorter);
      if (isRegistered) {
        try {
          ForceUpdateDropdown();
          logger.Info("Registered this to BetterSongList.");
        }
        catch (Exception exception) {
          logger.Error(exception);
        }
      }
      else {
        logger.Info("Failed to register sorter. Check AllowPluginSortsAndFilters config in BetterSongList.");
      }

      _registeredSorters.Add(plugin.name, sorter);
    }

    // On 1.34.2 BetterSongList doesn't update dropdown when a new sorter is registered.
    // https://github.com/kinsi55/BeatSaber_BetterSongList/issues/29
    private static void ForceUpdateDropdown() {
      var type = AccessTools.TypeByName("BetterSongList.UI.FilterUI");
      object ui = AccessTools.StaticFieldRefAccess<object>(type, "persistentNuts");
      AccessTools.Method(ui.GetType(), "UpdateTransformerOptionsAndDropdowns").Invoke(ui, null);
    }
  }

  public class FilterSortReference(FilterSortAdaptor sorter) : ITransformerPlugin, ISorterCustom, ISorterWithLegend {
    public string name => sorter.name;

    public bool visible => sorter.visible;

    public bool isReady => sorter.isReady;

    public void SetSorter(FilterSortAdaptor aSorter) {
      sorter = aSorter;
    }

    public void ContextSwitch(SelectLevelCategoryViewController.LevelCategory levelCategory, BaseLevelPack? playlist) {
      sorter.ContextSwitch(levelCategory, playlist);
    }

    public Task Prepare(CancellationToken cancelToken) {
      return sorter.Prepare(cancelToken);
    }

    public void DoSort(ref IEnumerable<BaseBeatmapLevel> levels, bool ascending) {
      sorter.DoSort(ref levels, ascending);
    }

    public IEnumerable<KeyValuePair<string, int>> BuildLegend(BaseBeatmapLevel[] levels) {
      return sorter.BuildLegend(levels);
    }
  }
}
