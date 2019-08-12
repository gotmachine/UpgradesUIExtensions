![UpgradesGUISExtended](https://img.shields.io/badge/KSP%20version-1.7.x-66ccff.svg?style=flat-square) 
![UpgradesGUISExtended](https://img.shields.io/badge/MOD%20version-1.5.0.2-orange.svg?style=flat-square)

# Upgrades GUI - Extended
Continuation of ![UpgradesUIExtensions](https://forum.kerbalspaceprogram.com/index.php?/topic/158081-122-upgradesgui-v15-upgrades-info-and-selection-in-vabsph-part-tooltips/) by (first by @GOT), now continued by *zer0Kerbal* with community support.

This plugin is a collections of interface tweaks aimed at making the part/module upgrades feature introduced in 1.2 more user-friendly.
Note that **the plugin doesn't add any upgrades**. If you want to have them in your game you need to download other mods that implement the upgrade feature.

![screenshot](https://raw.githubusercontent.com/gotmachine/UpgradesUIExtensions/902603cd0c7de45af979129f7af6f8fb839f912c/Documents/tooltipTweaks.png)

#### VAB/SPH part tool tips show upgraded stats
- The part stats are now updated according to unlocked upgrades.
- The part cost is now updated according to unlocked upgrades.
- All module widgets now show the updated stats according to unlocked upgrades.
- The part upgrade module widget show the detail of part stats/cost modifiers.
- If "showUpgradesInModuleInfo" (stock field) is set to true the upgrade config, the module widget now show the details of every upgrade currently unlocked for this module
- Some QOL tweaks to the tool tip stats : dry mass, mention of multi-mode engines, better formatting of engines thrust/ISP
- (bonus feature) Non-stock modules using cost/mass modifiers should have their modifiers taken into account too.

#### Upgrades selection
- This allow to customize which upgrades are applied to placed parts in all modes (Career, Science and Sandbox)
- Parts with upgrades now have a clickable "upgrade widget" in the tool tip widget list
- Clicking on the widget show a list of upgrade widgets that can be toggled to enable/disable upgrades for this part
- Upgrades exclusivity/overrides rules and R&D unlock status can't be bypassed
- Vessels with customized upgrades will work perfectly if the plug-in is removed, all this is done within the stock upgrade implementation.

#### R&D tech tree feature
- In the nodes part list, upgrades have a pale green background to better differentiate them from parts.

## Download & source

Soon to be available on **CKAN** !

**[LATEST RELEASE](https://github.com/zer0Kerbal/UpgradesUIExtensions/releases/latest)** and [source](https://github.com/zer0Kerbal/UpgradesUIExtensions) from github.

#### Disclaimer
I'm far from a skilled programmer, so the code for this may be ugly. As far as I know, it does the job and doesn't break the game. However, keep in mind that *I don't really know what I'm doing*. If anybody has the time to review and comment my code, I'm open to suggestions and pull requests :) *This also goes for me, zer0Kerbal. :)*

#### KSP-AVC disclaimer
This mod doesn't include mini-AVC, but it has a version file that allow version checking trough the [KSP-AVC Plugin](http://forum.kerbalspaceprogram.com/threads/79745).

#### Licensing
This masterful work of art is released under the [unlicense](http://unlicense.org/). 
So public domain, feel free to do anything, especially updating this plugin if I'm not around.

### See changelog for changes

### Known bugs and glitches
- None at the moment
- ***According to original author, GOT, even though you might be able to seemingly disable upgrades in the editor, these 'edits' may not and probably do not change anything in flight.***


