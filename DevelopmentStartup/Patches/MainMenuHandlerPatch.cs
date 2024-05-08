using System;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CTNOriginals.ContentWarning.DevelopmentStartup.Utilities;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;
using Object = System.Object;

namespace CTNOriginals.ContentWarning.DevelopmentStartup.Patches;

public class MainMenuHandlerPatch {
	public static void Patch() {
		On.MainMenuHandler.CheckConnected += CheckConnectedPatch;
		On.SpawnHandler.SpawnLocalPlayer += (orig, self, state) =>
		{
			orig(self, state);
			if(!PhotonNetwork.IsMasterClient) return;
			if(!PhotonGameLobbyHandler.IsSurface) return;
			CLogger.LogInfo("SpawnHandler.SpawnLocalPlayerPatch()");
			if (!Config.OldWorldModal.Value) return;
			ModalOption[] options = [
				new ModalOption("Yes", () =>
				{
					if(!Player.localPlayer.TryGetInventory(out PlayerInventory pInv)) return;
					if(!ItemDatabase.TryGetItemFromID(0x01, out Item item)) return;
					if(!pInv.TryGetSlotWithItemType(Item.ItemType.Camera, out _)) Item.EquipItem(item);
					
					PhotonGameLobbyHandler.Instance.photonView.RPC("RPC_StartTransition", RpcTarget.Others, Array.Empty<object>());
					RetrievableSingleton<PersistentObjectsHolder>.Instance.FindPersistantSurfaceObjects();
					RetrievableResourceSingleton<TransitionHandler>.Instance.TransitionToBlack(3f, delegate
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

			Modal.Show("Teleport to Old World", "Would you like to teleport to the old world?", options);
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
		}
	}
}