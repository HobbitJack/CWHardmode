# CWHardmode
 A BepInEx plugin for Cold Waters providing a more challenging experience

COLD WATERS: HARDMODE
Developed by HobbitJack
Version: 1.0.0

This plugin is a difficulty modifier for Cold Waters. 
It is designed for advanced players seeking a realistic experience above and beyond that found by default in the game.
Even though this is a difficulty plug-in, it is designed for use on the Realistic config setting, due to Elite adversly affecting vessel stats and sensor comparisons.
Due to using BepInEx/Harmony, it is theoretically possible that this plugin will work as an add-on for any mod of your choosing, however please note that it has only been tested with DotMod and the base game.

Features:
 - Disables 3D view, except for periscope (and pause menu, but we can pretend that works just fine)
 - Disables autoclassification of vessels, regardless of difficulty setting
 - Game will display the type of a torpedo when the icon is clicked on the map
 - Game will inform the player of hits on enemy vessels
 
Installation:
1. Download BepInEx V5.4.21x86 from the BepInEx GitHub page: https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x86_5.4.21.0.zip
2. Extract all items except the changelog to your Cold Waters directory (i.e. the BepInEx folder should be next to ColdWaters_Data)
3. Run the game. You should get a black screen.
4. Go to BepInEx/config/BepInEx.cfg, and under [Entry.Preloader], find Type = Application, and change that to Type = MonoBehaviour.
5. Run the game. It should launch normally.
6. Copy the CWHardmode.dll to BepInEx/plugins.
7. Launch the game. The plugin should be properly installed.

Note that this is a first version of this release; any issues found should be reported directly to HobbitJack.

Have fun!