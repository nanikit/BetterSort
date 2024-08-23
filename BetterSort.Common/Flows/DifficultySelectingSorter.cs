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
    ISortFilter sorter, SiraLog logger, ISongSelection songSelection, IDifficultySelector difficultySelector
  ) : ISortFilter {
    private ISorter? _hookingSorter;

    public event Action<ISortFilterResult?> OnResultChanged {
      add { sorter.OnResultChanged += value; }
      remove { sorter.OnResultChanged -= value; }
    }

    public string Name => sorter.Name;

    public void NotifyChange(IEnumerable<ILevelPreview> newLevels, bool isSelected = false) {
      sorter.NotifyChange(newLevels, isSelected);
      if (_hookingSorter == null) {
        _hookingSorter = songSelection.CurrentSorter;
        songSelection.OnSongSelected += SelectDifficulty;
      }
    }

    private async void SelectDifficulty(int index, LevelPreview preview) {
      try {
        if (_hookingSorter == null) {
          logger.Warn($"SelectDifficulty() is called while not hooking. Skip.");
          return;
        }
        if (songSelection.CurrentSorter != _hookingSorter) {
          logger.Debug($"Not selecting this sort while selecting difficulty. Unhook.");
          songSelection.OnSongSelected -= SelectDifficulty;
          _hookingSorter = null;
          sorter.NotifyChange([], false);
          return;
        }

        if (difficultySelector.SelectDifficulty(index, preview) is var (type, difficulty)) {
          await songSelection.SelectDifficulty(type, difficulty, preview).ConfigureAwait(false);
        }
      }
      catch (DirectoryNotFoundException notFound) {
        logger.Info($"Suppress IO exception, it may occur when delete beatmap; {notFound.Message}");
      }
      catch (Exception ex) {
        logger.Error(ex);
      }
    }
  }
}
