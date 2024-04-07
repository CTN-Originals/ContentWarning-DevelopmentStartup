using BepInEx;
using System.Collections;

using CTNOriginals.ContentWarning.DevelopmentStartup.Utilities;

namespace CTNOriginals.ContentWarning.DevelopmentStartup.Patches;

public class MainMenuHandlerPatch {
	public static void Patch() {
		On.MainMenuHandler.CheckConnected += CheckConnectedPatch;
	}

	static readonly string errorLogEnd = $"\nPlease correct this in the config ({Paths.ConfigPath + "\\" + Plugin.PluginGUID + ".cfg"}).";
	static bool hasConnected = false;
	private static IEnumerator CheckConnectedPatch(On.MainMenuHandler.orig_CheckConnected orig, MainMenuHandler self) {
		IEnumerator origEnum = orig(self);
		while (origEnum.MoveNext()) yield return origEnum.Current;
		if (!hasConnected) {
			CLogger.LogInfo("MainMenuHandler.CheckConnectedPatch()");
			if (int.TryParse(Config.SaveGameIndex.Value, out int saveIndex)) {
				if (saveIndex < 1 || saveIndex > 3) {
					CLogger.LogError($"Config.SaveGameIndex is set to {saveIndex} and is out of range (1 - 3), defaulting to 1.{errorLogEnd}");
					saveIndex = 1;
				}
				self.Host(saveIndex - 1);
			}
			else {
				CLogger.LogError($"Config.SaveGameIndex was not able to be parsed to an integer, value: ({Config.SaveGameIndex.Value}).{errorLogEnd}");
				self.Host(0);
			}
			hasConnected = true;
		}
	}
}