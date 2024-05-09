using BepInEx;
using System.Collections;
using System.Collections.Generic;
using CTNOriginals.ContentWarning.DevelopmentStartup.Utilities;
using Photon.Pun;
using Zorro.Core;

namespace CTNOriginals.ContentWarning.DevelopmentStartup.Patches;

public class MainMenuHandlerPatch {
	public static void Patch() {
		On.MainMenuHandler.CheckConnected += CheckConnectedPatch;
		On.SpawnHandler.SpawnLocalPlayer += (orig, self, state) =>
		{
			orig(self, state);
			if(!PhotonNetwork.IsMasterClient
			   || !PhotonGameLobbyHandler.IsSurface
			   || PhotonGameLobbyHandler.CurrentObjective.GetType() != typeof(InviteFriendsObjective)
			   || !Config.OldWorldModal.Value) return;

			ShowOurModal();
		};
	}

	static readonly string errorLogEnd = $"\nPlease correct this in the config ({Paths.ConfigPath + "\\" + Plugin.PluginGUID + ".cfg"}).";
	static bool hasConnected = false;
	
	private static IEnumerator CheckConnectedPatch(On.MainMenuHandler.orig_CheckConnected orig, MainMenuHandler self) {
		IEnumerator origEnum = orig(self);
		while (origEnum.MoveNext()) yield return origEnum.Current;
		if (!hasConnected) {
			hasConnected = true;
			CLogger.LogInfo("MainMenuHandler.CheckConnectedPatch()");
			if (int.TryParse(Config.SaveGameIndex.Value, out var saveIndex)) {
				if (saveIndex is < 1 or > 3) {
					CLogger.LogError($"Config.SaveGameIndex is set to {saveIndex} and is out of range (1 - 3), defaulting to 1.{errorLogEnd}");
					saveIndex = 1;
				}
				self.Host(saveIndex - 1);
			}
			else {
				CLogger.LogError($"Config.SaveGameIndex was not able to be parsed to an integer, value: ({Config.SaveGameIndex.Value}).{errorLogEnd}");
				self.Host(0);
			}
		}
	}

	private static void ShowOurModal()
	{
		ModalOption[] options = [
			new ModalOption("Yes", () =>
			{
				if(!Player.localPlayer.TryGetInventory(out var pInventory)) return;
				if(!ItemDatabase.TryGetItemFromID(0x01, out var item)) return;
				if(!pInventory.TryGetSlotWithItemType(Item.ItemType.Camera, out _)) Item.EquipItem(item);
					
				RetrievableSingleton<PersistentObjectsHolder>.Instance.FindPersistantSurfaceObjects();
				RetrievableResourceSingleton<TransitionHandler>.Instance.TransitionToBlack(2f, delegate
				{
					var text = Config.MapToTeleportTo.Value switch
					{
						Config.MapName.Factory => "Factory",
						Config.MapName.Harbour => "Harbour",
						Config.MapName.Mines => "Mines",
						_ => new List<string> { "FactoryScene", "MinesScene", "HarbourScene" }[
							SurfaceNetworkHandler.RoomStats.LevelToPlay]
					};
						
					PhotonNetwork.LoadLevel(text);
				}, 0f);
			}),
			new ModalOption("No"),
			new ModalOption("No\nDon't Show Again", () => Config.OldWorldModal.Value = false)
		];

		Modal.Show("Teleport to Old World", "Would you like to teleport to the old world? Doing this will give you a camera.", options);
	}
}