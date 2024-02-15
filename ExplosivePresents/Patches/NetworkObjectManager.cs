using System.IO;
using System.Reflection;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Hypick.Patches;

[HarmonyPatch]
public class NetworkObjectManager
{
	[HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
	public static void Init()
	{
		if (networkPrefab != null)
			return;

		// var MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets/netcodemod"));
		var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets/netcodemod");
		AssetBundle MainAssetBundle;
		if (File.Exists(path))
			MainAssetBundle = AssetBundle.LoadFromFile(path);
		else
			MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod"));

		Plugin.Log.LogInfo($"Loading NetworkManager Prefab: {MainAssetBundle}");
		networkPrefab = (GameObject)MainAssetBundle.LoadAsset("Assets/Necode/NetwordManager.prefab");
		networkPrefab.AddComponent<NetworkHandler>();

		NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
	}

	[HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
	static void SpawnNetworkHandler()
	{
		if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
		{
			var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
			networkHandlerHost.GetComponent<NetworkObject>().Spawn();
		}
	}

	static GameObject networkPrefab;
}
