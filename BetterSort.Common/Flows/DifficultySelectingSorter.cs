using BetterSongList.SortModels;
using BetterSort.Common.External;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterSort.Common.Flows {

  public interface IDifficultySelector {

    PlayedMap? SelectDifficulty(int index, LevelPreview preview);
  }

  /// <summary>
  /// Support isSelected of NotifyChange and select difficulty when song is selected.
  /// </summary>
  public class DifficultySelectingSorter(
    ISortFilter sorter, SiraLog logger, ISongSelection songSelection, IDifficultySelector selector
  ) : ISortFilter {
    private readonly ISortFilter _sorter = sorter;
    private readonly SiraLog _logger = logger;
    private readonly ISongSelection _songSelection = songSelection;
    private readonly IDifficultySelector _difficultySelector = selector;
    private ISorter? _hookingSorter;

    public event Action<ISortFilterResult?> OnResultChanged {
      add { _sorter.OnResultChanged += value; }
      remove { _sorter.OnResultChanged -= value; }
    }

    public string Name => _sorter.Name;

    public void NotifyChange(IEnumerable<ILevelPreview> newLevels, bool isSelected = false) {
      _sorter.NotifyChange(newLevels, isSelected);
      if (_hookingSorter == null) {
        _hookingSorter = _songSelection.CurrentSorter;
        _songSelection.OnSongSelected += SelectDifficulty;
      }
    }

    private async void SelectDifficulty(int index, LevelPreview preview) {
      try {
        if (_hookingSorter == null) {
          _logger.Warn($"SelectDifficulty() is called while not hooking. Skip.");
          return;
        }
        if (_songSelection.CurrentSorter != _hookingSorter) {
          _logger.Debug($"Not selecting this sort while selecting difficulty. Unhook.");
          _songSelection.OnSongSelected -= SelectDifficulty;
          _hookingSorter = null;
          _sorter.NotifyChange([], false);
          return;
        }

        if (_difficultySelector.SelectDifficulty(index, preview) is var (type, difficulty)) {
          await _songSelection.SelectDifficulty(type, difficulty, preview).ConfigureAwait(false);
        }
      }
      catch (DirectoryNotFoundException notFound) {
        _logger.Info($"Suppress IO exception, it may occur when delete beatmap; {notFound.Message}");
      }
      catch (Exception ex) {
        _logger.Error(ex);
      }
    }
  }
}
