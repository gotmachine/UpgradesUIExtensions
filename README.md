# Upgrades UI extensions

This plugin is a collections of user interface tweaks aimed at making the part/module upgrades feature introduced in 1.2 a bit less painful to use for the player :

![screenshot](https://github.com/gotmachine/UpgradesUIExtensions/raw/3b84a046bdf9de080792d63a51e21ad47e8cc153/Documents/tooltipTweaks.png)

#### VAB/SPH part tooltips show upgraded stats
- The part stats are now updated according to unlocked upgrades.
- The part cost is now updated according to unlocked upgrades.
- All module widgets now show the updated stats according to unlocked upgrades.
- The part upgrade module widget show the detail of part stats/cost modifiers.
- If "showUpgradesInModuleInfo" (stock field) is set to true the upgrade config, the module widget now show the details of every upgrade currently unlocked for this module
- Some QOL tweaks to the tooltip stats : dry mass, mention of multi-mode engines, better formatting of engines thrust/ISP
- (bonus feature) Non-stock modules using cost/mass modifiers should have their modifiers taken into account too.

#### Upgrades selection
- This allow to customize which upgrades are applied to placed parts in all modes (Career, Science and Sandbox)
- Parts with upgrades now have a clickable "upgrade widget" in the tooltip widget list
- Clicking on the widget show a list of upgrade widgets that can be toggled to enable/disable upgrades for this part
- Upgrades exclusivity/overrides rules and R&D unlock status can't be bypassed
- Vessels with customized upgrades will work perfectly if the plugin is removed, all this is done within the stock upgrade implementation.

#### R&D tech tree feature
- In the nodes part list, upgrades have a pale green background to better differentiate them from parts.

## Download & source

Soon to be available on **CKAN** !

**[LATEST RELEASE](https://github.com/gotmachine/UpgradesUIExtensions/releases/latest)** and [source](https://github.com/gotmachine/UpgradesUIExtensions) from github.

#### Disclaimer
I'm far from a skilled programmer, so the code for this may be ugly. As far as I know, it does the job and doesn't break the game. However, keep in mind that *I don't really know what I'm doing*. If anybody has the time to review and comment my code, I'm open to suggestions and pull requests :)

#### KSP-AVC disclaimer
This mod doesn't include mini-AVC, but it has a version file that allow version checking trough the [KSP-AVC Plugin](http://forum.kerbalspaceprogram.com/threads/79745).

#### Licensing
This masterful work of art is released under the [unlicense](http://unlicense.org/). 
So public domain, feel free to do anything, especially updating this plugin if I'm not around.

## Changelog and bugs

#### Known bugs and glitches
- None at the moment

#### v1.4 for KSP 1.2.2 - 28/03/2017

- New feature : upgrade selection system
- Refactored a lot of things
- Re-fixed nullref on creating the upgraded parts prefab (thanks @Oort for the perfect bug report)
- Removed mini-AVC dll, KSP-AVC is still supported

#### v1.3 for KSP 1.2.2 - 28/03/2017

- Fixed an issue causing an exception within the GameDatabase, this resolve the issue with toolbar icons disappearance (Thanks @Rodger)
- The module widget list in the part tooltip is now sorted alphabetically (this reproduce the stock behaviour)

#### v1.3 for KSP 1.2.2 - 28/03/2017

- Fixed an issue causing an exception within the GameDatabase, this resolve the issue with toolbar icons disappearance (Thanks @Rodger)
- The module widget list in the part tooltip is now sorted alphabetically (this reproduce the stock behaviour)

#### v1.2 for KSP 1.2.2 - 24/03/2017

- The "custom prefabs" parts now try to call OnLoad() on their modules, with the HighLogic.LoadedScene set to LOADING, in an effort to better replicate what happens with the real part prefabs. This fix the issue with Kerbalism custom modules, and may prevent the same kind of error from surfacing in other plugins. Thanks @ShotgunNinja for guidance on what was happening.
- Added some error-checking so if things go wrong, the plugin should fail a bit more gracefully.

#### v1.1 for KSP 1.2.2 - 23/03/2017
- Added KSP-AVC support for version checking

#### v1.0 for KSP 1.2.2 - 22/03/2017
- Initial release
