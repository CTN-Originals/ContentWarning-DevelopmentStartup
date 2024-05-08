using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CTNOriginals.ContentWarning.DevelopmentStartup.Utilities;

namespace CTNOriginals.ContentWarning.DevelopmentStartup;

public class Config {
	public static ConfigFile Configurations = new ConfigFile(Paths.ConfigPath + "\\" + Plugin.PluginGUID + ".cfg", true);

	public enum MapName {
		Random,
		Factory,
		Harbour,
		Mines
	}
	
	public static ConfigEntry<string> SaveGameIndex;
	public static ConfigEntry<bool> OldWorldModal;
	public static ConfigEntry<MapName> MapToTeleportTo;

	public void LoadFile() {
		SaveGameIndex = Configurations.Bind("General", "SaveGameIndex", "1", "The number of the saved game to load on startup (a number between 1 and 3)");
		OldWorldModal = Configurations.Bind("General", "OldWorldModal", true, "Whether or not to show a prompt to teleport to old world with a Camera when spawning in.");
		MapToTeleportTo = Configurations.Bind("General", "MapToTeleportTo", MapName.Random, "The map to teleport to when teleporting to the old world.");
		
		CLogger.LogInfo($"SaveGameIndex: {SaveGameIndex.Value}");
		CLogger.LogInfo($"OldWorldModal: {OldWorldModal.Value}");
		CLogger.LogInfo($"MapToTeleportTo: {MapToTeleportTo.Value}");

		Configurations.Save();
	}
}