using BetterSort.Accuracy.External;
using BetterSort.Common.Flows;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.Accuracy.Sorter {

  public class UIAwareSorter : ISortFilter, IDifficultySelector {
    private readonly AccuracySorter _sorter;
    private readonly SiraLog _logger;
    private readonly IBsInterop _bsInterop;

    public UIAwareSorter(AccuracySorter sorter, SiraLog logger, IBsInterop bsInterop) {
      _sorter = sorter;
      _logger = logger;
      _bsInterop = bsInterop;

      _sorter.OnResultChanged += UpdatePlaylistManager;
    }

    public event Action<ISortFilterResult?> OnResultChanged {
      add { _sorter.OnResultChanged += value; }
      remove { _sorter.OnResultChanged -= value; }
    }

    public string Name => _sorter.Name;

    public void NotifyChange(IEnumerable<ILevelPreview> newLevels, bool isSelected = false) {
      _sorter.NotifyChange(newLevels, isSelected);
    }

    public PlayedMap? SelectDifficulty(int index, LevelPreview preview) {
      if (index < 0 || _sorter.Mapping.Count <= index) {
        _logger.Debug($"User picked a song never played so skip selecting difficulty. ({index} {preview.LevelId} {preview.SongName})");
        return null;
      }

      var record = _sorter.Mapping[index];
      _logger.Debug($"{nameof(SelectDifficulty)}: Set {index} {preview.SongName} {record.Mode} {record.Difficulty} {record.Accuracy}");
      return new PlayedMap(record.Mode, record.Difficulty);
    }

    private void UpdatePlaylistManager(ISortFilterResult? result) {
      if (result != null) {
        _bsInterop.SetPlaylistItem(result.Levels.OfType<LevelPreview>().Select(x => x.Preview).ToList());
      }
    }
  }
}
