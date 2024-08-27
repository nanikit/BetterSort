# BetterSort

This repository contains additional beat saber sort plugins can be used with [BetterSongList](https://github.com/kinsi55/BeatSaber_BetterSongList).
All plugins here require BetterSongList.

## BetterSort.LastPlayed

<img src="docs/preview.webp" alt="last played sorter screen" width="300"/>

This plugin records your last played date and sorts songs accordingly.

## BetterSort.Accuracy

<img src="docs/accuracy-preview.webp" alt="accuracy sorter screen" height="300"/>

It sorts by difficulty according to your best accuracy.

## Installation

You can use [ModAssistant](https://github.com/Assistant/ModAssistant/releases/latest) for installation, which I fully support.

If plugins isn't available on ModAssistant or new features are added later, you can [download plugins manually](https://github.com/nanikit/BetterSort/releases).

When manually downloading mods, please be careful of the mod's dependencies. Currently BS_Utils.dll, SiraUtil.dll, and BetterSongList.dll are required.

## Usage

Simply click the left-bottom sort button in the song select scene, and choose either 'Last played' or 'Accuracy'.

## Q&A

- Q: Just installed, it doesn't sort at all.

  A: For LastPlayed, immediately after installation, there's no play history recorded. So, play some songs first.<br />

  For Accuracy, the plugin gathers your scores from ScoreSaber and BeatLeader upon first launch. This process may take a while depending on your score history.

- Q: Reverse sort doesn't work.<br />
  A: I intentionally didn't support reverse sorting. Currently BetterSongList doesn't remember the sort direction of each sorter, which caused confusion for me while playing.
