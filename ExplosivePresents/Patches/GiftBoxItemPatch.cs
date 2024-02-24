using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Hypick.Patches;

[HarmonyPatch(typeof(GiftBoxItem))]
internal class GiftBoxItemPatch
{
	[HarmonyPatch("OpenGiftBoxServerRpc")]
	[HarmonyPrefix]
	public static bool OpenGiftBox(GiftBoxItem __instance)
	{
		if (!(Mathf.Clamp(Plugin.Config.SpawnChance, 0f, 100f) / 100f > Random.Range(0f, 0.99f)))
			return OpenGiftBoxCustom(__instance);

		NetworkHandler.Instance.CustomExplodeMineServerRpc(__instance.transform.position);
		return OpenGiftBoxCustom(__instance, true);
	}

	private static bool OpenGiftBoxCustom(GiftBoxItem __instance, bool explosive = false)
	{
		NetworkManager networkManager = __instance.NetworkManager;
		if (networkManager == null || !networkManager.IsListening)
			return false;

		if (GetPrivateField<RpcExecStage>(__instance, "__rpc_exec_stage") != RpcExecStage.Server &&
		    (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = new();

			FastBufferWriter bufferWriter = (FastBufferWriter)GetPrivateMethod(__instance, "__beginSendServerRpc",
				[2878544999u, serverRpcParams, RpcDelivery.Reliable]);
			GetPrivateMethod(__instance, "__endSendServerRpc",
				[bufferWriter, 2878544999u, serverRpcParams, RpcDelivery.Reliable]);
		}

		if (GetPrivateField<RpcExecStage>(__instance, "__rpc_exec_stage") != RpcExecStage.Server ||
		    (!networkManager.IsClient && !networkManager.IsHost))
			return false;

		if (explosive)
		{
			Plugin.Log.LogInfo("Opening a gift with a bomb inside :)");
			__instance.OpenGiftBoxNoPresentClientRpc();
			return false;
		}

		Plugin.Log.LogInfo("Opening a gift without a bomb :(");

		GameObject gameObject = null;
		var presentValue = 0;
		Vector3 vector = Vector3.zero;
		GameObject objectInPresent = GetPrivateField<GameObject>(__instance, "objectInPresent");

		if (objectInPresent == null)
			Debug.LogError("Error: There is no object in gift box!");
		else
		{
			Transform parent;
			if (((__instance.playerHeldBy != null && __instance.playerHeldBy.isInElevator) ||
			     StartOfRound.Instance.inShipPhase) && RoundManager.Instance.spawnedScrapContainer != null)
				parent = RoundManager.Instance.spawnedScrapContainer;
			else
				parent = StartOfRound.Instance.elevatorTransform;

			vector = __instance.transform.position + Vector3.up * 0.25f;
			gameObject = Object.Instantiate(objectInPresent, vector, Quaternion.identity, parent);
			GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
			PlayerControllerB previousPlayerHeldBy =
				GetPrivateField<PlayerControllerB>(__instance, "previousPlayerHeldBy");

			component.startFallingPosition = vector;
			__instance.StartCoroutine(__instance.SetObjectToHitGroundSFX(component));
			component.targetFloorPosition = component.GetItemFloorPosition(__instance.transform.position);
			if (previousPlayerHeldBy != null && previousPlayerHeldBy.isInHangarShipRoom)
				previousPlayerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, component);

			presentValue = GetPrivateField<int>(__instance, "objectInPresent");
			component.SetScrapValue(presentValue);
			component.NetworkObject.Spawn();
		}

		if (gameObject != null)
			__instance.OpenGiftBoxClientRpc(gameObject.GetComponent<NetworkObject>(), presentValue, vector);

		__instance.OpenGiftBoxNoPresentClientRpc();
		return false;
	}

	private enum RpcExecStage
	{
		Server
	}

	private static object GetPrivateMethod(object instance, string methodName, params object[] args)
	{
		const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
		MethodInfo field = instance.GetType().GetMethod(methodName, bindingFlags);

		return field?.Invoke(instance, args);
	}

	private static T GetPrivateField<T>(object instance, string fieldName)
	{
		const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
		FieldInfo field = instance.GetType().GetField(fieldName, bindingFlags);

		return (T)field?.GetValue(instance);
	}
}