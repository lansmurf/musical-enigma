using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    // A marker to prevent spawning multiple shopkeepers at one campfire
    public class HasBingBongShop : MonoBehaviour {} 

    [HarmonyPatch(typeof(Campfire))]
    public static class CampfirePatches
    {
        [HarmonyPatch("Light_Rpc")]
        [HarmonyPostfix]
        public static void SpawnShopkeeperAtCampfire(Campfire __instance)
        {
            // Prevent spawning if one already exists here or if the main BingBong isn't available
            if (__instance.GetComponent<HasBingBongShop>() != null || BingBong.Instance == null)
            {
                return;
            }

            CoinPlugin.Log.LogInfo($"Campfire lit! Spawning temporary Shopkeeper BingBong at {__instance.transform.position}.");

            GameObject originalBingBong = BingBong.Instance.gameObject;
            
            // Spawn the shopkeeper slightly behind the campfire
            Vector3 spawnPosition = __instance.transform.position - __instance.transform.forward * 2f;

            // Use the same helper function from our other patch
            WorldSpawnPatches.CreateShopkeeper(originalBingBong, spawnPosition);

            // "Tag" this campfire so we don't spawn another shopkeeper here
            __instance.gameObject.AddComponent<HasBingBongShop>();
        }
    }
}