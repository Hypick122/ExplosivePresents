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
        if (Mathf.Clamp(Plugin.Config.SpawnChance, 0f, 100f) / 100f > Random.Range(0f, 0.99f))
        {
            NetworkHandler.Instance.CustomExplodeMineServerRpc(__instance.transform.position);
            return OpenGiftBoxCustom(__instance, true);
        }
        return OpenGiftBoxCustom(__instance);
    }

	private static bool OpenGiftBoxCustom(GiftBoxItem __instance, bool explosive = false)
	{
		NetworkManager networkManager = __instance.NetworkManager;
		if (networkManager == null || !networkManager.IsListening)
			return false;

        if ((RpcExecStage)__instance.GetType().GetField("__rpc_exec_stage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) != RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
        {
            ServerRpcParams serverRpcParams = new();
            FastBufferWriter bufferWriter = (FastBufferWriter)__instance.GetType().GetMethod("__beginSendServerRpc", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, [2878544999u, serverRpcParams, RpcDelivery.Reliable]);
            __instance.GetType().GetMethod("__endSendServerRpc", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, [bufferWriter, 2878544999u, serverRpcParams, RpcDelivery.Reliable]);
        }
        if ((RpcExecStage)__instance.GetType().GetField("__rpc_exec_stage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) != RpcExecStage.Server || (!networkManager.IsClient && !networkManager.IsHost))
            return false;

        if (explosive) {
            Plugin.Log.LogInfo("OPEN GIFTBOX WTTH BOMB AHAHAAHAAH");
            __instance.OpenGiftBoxNoPresentClientRpc();
            return false;
        }

        Plugin.Log.LogInfo("OPEN GIFTBOX without bomb :(");

        GameObject gameObject = null;
        int presentValue = 0;
        Vector3 vector = Vector3.zero;
        GameObject objectInPresent = (GameObject)__instance.GetType().GetField("objectInPresent", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

        if (objectInPresent == null)
            Debug.LogError("Error: There is no object in gift box!");
        else
        {
            // Transform parent = ((((!(__instance.playerHeldBy != null) || !__instance.playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer);
            Transform parent;
            if (((__instance.playerHeldBy != null && __instance.playerHeldBy.isInElevator) || StartOfRound.Instance.inShipPhase) && RoundManager.Instance.spawnedScrapContainer != null)
                parent = RoundManager.Instance.spawnedScrapContainer;
            else
                parent = StartOfRound.Instance.elevatorTransform;

            vector = __instance.transform.position + Vector3.up * 0.25f;
            gameObject = Object.Instantiate(objectInPresent, vector, Quaternion.identity, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            PlayerControllerB previousPlayerHeldBy = (PlayerControllerB)__instance.GetType().GetField("previousPlayerHeldBy", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            component.startFallingPosition = vector;
            __instance.StartCoroutine(__instance.SetObjectToHitGroundSFX(component));
            component.targetFloorPosition = component.GetItemFloorPosition(__instance.transform.position);
            if (previousPlayerHeldBy != null && previousPlayerHeldBy.isInHangarShipRoom)
            previousPlayerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, component);

            presentValue = (int)__instance.GetType().GetField("objectInPresentValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
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
        None,
        Server,
        Client
    }
}
