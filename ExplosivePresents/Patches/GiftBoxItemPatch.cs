using System.Collections;
using ExplosivePresents.Core;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace ExplosivePresents.Patches
{
    [HarmonyPatch(typeof(GiftBoxItem))]
    internal class GiftBoxItemPatch
    {
        static IEnumerator DelayedExplosion(GiftBoxItem __instance, bool effect, float killrange, float damagerange, float delay)
        {
            Landmine landmine = Resources.FindObjectsOfTypeAll<Landmine>()[0];
            AudioSource audioSource = __instance.gameObject.GetComponent<AudioSource>();
            audioSource.PlayOneShot(landmine.mineTrigger, 1f);

            yield return new WaitForSeconds(delay);

            audioSource.PlayOneShot(landmine.mineDetonate, 1f);
            Landmine.SpawnExplosion(__instance.transform.position, spawnExplosionEffect: effect, killrange, damagerange);
        }

        [HarmonyPatch("OpenGiftBoxServerRpc")]
        [HarmonyPostfix]
        static void OpenGiftBox(GiftBoxItem __instance)
        {
            Random random = new Random();

            if (random.Next(0, 100) <= Plugin.Config.SpawnChance)
            {
                __instance.StartCoroutine(DelayedExplosion(__instance, true, Plugin.Config.KillRange, Plugin.Config.DamageRange, Plugin.Config.Delay));
            }
        }
    }

    // [HarmonyPatch(typeof(PlayerControllerB))]
    // internal class DebugPatch
    // {
    //     [HarmonyPatch("Update")]
    //     [HarmonyPostfix]
    //     static void DebugUpdatePatch(PlayerControllerB __instance)
    //     {
    //         if (UnityInput.Current.GetKeyDown("o"))
    //         {
    //             Plugin.Logger.LogInfo("Update");
    //             GameObject gameObject = Object.Instantiate<GameObject>(Resources.FindObjectsOfTypeAll<GiftBoxItem>()[0].gameObject, __instance.gameObject.transform.position, Quaternion.identity);
    //             gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
    //             gameObject.AddComponent<NetworkObject>();
    //             gameObject.GetComponent<NetworkObject>().Spawn(false);
    //         }
    //     }
    // }
}
