using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Hypick;

public class NetworkHandler : NetworkBehaviour
{
	public static NetworkHandler Instance { get; private set; }

	[ServerRpc(RequireOwnership = false)]
	public void CustomExplodeMineServerRpc(Vector3 explosionPosition)
	{
		NetworkManager networkManager = base.NetworkManager;

		if (networkManager == null || !networkManager.IsListening)
			return;

		if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost)) { }

		if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
		{
			CustomExplodeMineClientRpc(explosionPosition);
		}
	}

	[ClientRpc]
	public void CustomExplodeMineClientRpc(Vector3 explosionPosition)
	{
		NetworkManager networkManager = base.NetworkManager;
		if (networkManager == null || !networkManager.IsListening)
			return;

		if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost)) { }

		if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
		{
			base.StartCoroutine(DelayedExplosion(explosionPosition, true, Plugin.Config.KillRange, Plugin.Config.DamageRange, Plugin.Config.Delay));
		}
	}

	static IEnumerator DelayedExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f, float Delay = 1)
	{
		Landmine landmine = Resources.FindObjectsOfTypeAll<Landmine>()[0];

		// if (Plugin.Config.ImmediateExplosion) {
		//     GameObject gameObject = Instantiate(landmine.gameObject, explosionPosition, Quaternion.identity);
		//     gameObject.GetComponent<GrabbableObject>().fallTime = 1f;
		//     gameObject.transform.position = explosionPosition;
		//     gameObject.transform.forward = new Vector3(1, 0, 0);
		//     gameObject.GetComponent<NetworkObject>().Spawn(true);
		// } else {
		GameObject mineAudio = GameObject.Find("SpeakerAudio");
		mineAudio.GetComponent<AudioSource>().PlayOneShot(landmine.mineTrigger, 1f);

		yield return new WaitForSeconds(Delay);

		mineAudio.GetComponent<AudioSource>().PlayOneShot(landmine.mineDetonate, 1f);
		Landmine.SpawnExplosion(explosionPosition + Vector3.up, spawnExplosionEffect: spawnExplosionEffect, killRange, damageRange);
		// }
	}

	public override void OnNetworkSpawn()
	{
		if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
			Instance?.gameObject.GetComponent<NetworkObject>().Despawn();

		if (Instance == null)
			Instance = this;
		base.OnNetworkSpawn();
	}
}
