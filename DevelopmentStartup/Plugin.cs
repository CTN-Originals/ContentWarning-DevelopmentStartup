using BepInEx;
using BepInEx.Logging;

namespace CTNOriginals.ContentWarning.DevelopmentStartup;

[BepInPlugin("com.ctnoriginals.cw.developmentstartup", "DevelopmentStartup", "1.0.0")]
public class Plugin : BaseUnityPlugin {
	public static ManualLogSource ManualLog;

	public static string PluginGUID = "com.ctnoriginals.cw.developmentstartup";
	public static string PluginName = "DevelopmentStartup";
	public static string PluginVersion = "1.0.0";

	public static bool DebugMode = false;
	public static bool OutputDebugLogs = false;

	public static Config ConfigFile = new();

	private void Awake() {
		#if DEBUG
			DebugMode = true;
		#endif
		
		ManualLog = Logger;
		ManualLog.LogInfo($"Plugin {PluginName} is loaded! Version: {PluginVersion} ({(DebugMode ? "Debug" : "Release")})");
		
		ConfigFile.LoadFile();

		Patches.MainMenuHandlerPatch.Patch();
	}
}