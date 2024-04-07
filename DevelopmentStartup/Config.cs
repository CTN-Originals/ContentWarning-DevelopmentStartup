using BepInEx;
using BepInEx.Configuration;

using CTNOriginals.ContentWarning.DevelopmentStartup.Utilities;

namespace CTNOriginals.ContentWarning.DevelopmentStartup;

public class Config {
	public static ConfigFile Configurations = new ConfigFile(Paths.ConfigPath + "\\" + Plugin.PluginGUID + ".cfg", true);

	public static ConfigEntry<string> SaveGameIndex;

	public void LoadFile() {
		SaveGameIndex = Configurations.Bind("General", "SaveGameIndex", "1", "The number of the saved game to load on startup (a number between 1 and 3)");
		
		CLogger.LogInfo($"SaveGameIndex: {SaveGameIndex.Value}");

		Configurations.Save();
	}
}