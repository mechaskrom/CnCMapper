# CnCMapper
A command line tool that can save map images of Command &amp; Conquer games. Currently it supports Dune II, Tiberian Dawn and Red Alert.

Radar (minimap) images of maps can also be saved if wanted.

## About
I wrote it mostly for making my contributions to [vgmaps](https://vgmaps.com/):
- [Dune II](https://vgmaps.com/Atlas/PC/index.htm#DuneIIBuildingOfADynasty)
- [Tiberian Dawn](https://vgmaps.com/Atlas/PC/index.htm#CommandConquerTiberianDawn)
- [Red Alert](https://vgmaps.com/Atlas/PC/index.htm#CommandConquerRedAlert)

Primarily I just wanted maps of Red Alert missions, but it was easier to first make the tool handle Tiberian Dawn. Red Alert is very similar to it and is essentially just a little more complicated expansion/superset of it.

Dune II was added later on mainly because it was pretty easy, as it also is very similar to Tiberian Dawn.

## Usage
A config file (.INI file) can be passed to the tool to control how it should draw/output maps. Run the tool without any arguments to generate default config files.
```
CnCMapper infolder [config file]

Infolder is path to a supported game folder.
Config file is an optional path to a configfile. Try to detect game and use its default config if omitted.

Example: CnCMapper ".\Command & Conquer(tm)" "TiberianDawn.ini"
```

## Required Files
A complete game folder is not needed. Here is a list of files CnCMapper will look for:
```
A Dune II: The Building of a Dynasty / Battle for Arrakis (D2) folder:
-needs 'DUNE.PAK'
-maps (.INI) searched for in 'SCENARIO.PAK' and the folder

A Command & Conquer aka Tiberian Dawn (TD) folder:
-needs 'CONQUER.MIX'
-optionally 'DESERT.MIX'
-optionally 'TEMPERAT.MIX'
-optionally 'WINTER.MIX'
-maps (.INI+.BIN) searched for in 'GENERAL.MIX', 'SC-000.MIX', 'SC-001.MIX' and the folder
-multiplayer maps should start with 'SCM' and end with '.INI|.MPR' e.g. 'SCM01EA.INI'

A Command & Conquer Red Alert (RA) folder:
-needs 'CONQUER.MIX' (folder or inside 'MAIN.MIX')
-needs 'HIRES.MIX' or 'LORES.MIX' (folder or inside 'REDALERT.MIX')
-needs 'LOCAL.MIX' (folder or inside 'REDALERT.MIX')
-optionally 'INTERIOR.MIX' (folder or inside 'MAIN.MIX')
-optionally 'SNOW.MIX' (folder or inside 'MAIN.MIX')
-optionally 'TEMPERAT.MIX' (folder or inside 'MAIN.MIX')
-optionally 'EXPAND.MIX'
-optionally 'EXPAND2.MIX'
-optionally 'HIRES1.MIX' or 'LORES1.MIX'
-maps (.INI) searched for in 'GENERAL.MIX' (folder or inside 'MAIN.MIX') and the folder
-multiplayer maps should start with 'SCM' and end with '.INI|.MPR' e.g. 'SCM01EA.INI'
```

## Example Output
![Dune II](https://github.com/mechaskrom/CnCMapper/blob/main/examples/D2%20-%20Atreides%20-%20SCENA022%20-%209A.png?raw=true)
![Tiberian Dawn](https://github.com/mechaskrom/CnCMapper/blob/main/examples/TD%20-%20GDI%20-%20SCG02EA%20-%20Knock%20Out%20That%20Refinery.png?raw=true)
![Red Alert](https://github.com/mechaskrom/CnCMapper/blob/main/examples/RA%20-%20Allies%20-%20SCG01EA%20-%20In%20The%20Thick%20Of%20It.png?raw=true)
![Red Alert radar](https://github.com/mechaskrom/CnCMapper/blob/main/examples/RA%20-%20Soviet%20-%20SCU09EA%20-%20Liability%20Elimination%20%5Bscale%203%5D.png?raw=true)
