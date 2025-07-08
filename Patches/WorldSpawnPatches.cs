using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class WorldSpawnPatches
    {
        private static bool hasSpawnedPermanentShop = false;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void SpawnPermanentShop(Player __instance)
        {
            if (!__instance.photonView.IsMine || hasSpawnedPermanentShop || BingBong.Instance == null)
            {
                return;
            }

            CoinPlugin.Log.LogInfo("Spawning permanent Shopkeeper BingBong...");

            GameObject originalBingBong = BingBong.Instance.gameObject;
            Vector3 spawnPosition = originalBingBong.transform.position + new Vector3(5f, 0, 0);

            CreateShopkeeper(originalBingBong, spawnPosition);
            
            hasSpawnedPermanentShop = true;
        }

        // This is our central function for creating ANY shopkeeper.
        public static void CreateShopkeeper(GameObject originalPrefab, Vector3 position)
        {
            GameObject shopkeeper = Object.Instantiate(originalPrefab, position, Quaternion.identity);
            shopkeeper.name = "Shopkeeper BingBong";
            
            Object.Destroy(shopkeeper.GetComponent<Animator>());

            // --- NEW: CHANGE THE COLOR TO BLUE ---
            
            // Find the renderer component. It's likely a SkinnedMeshRenderer on a child object.
            var renderer = shopkeeper.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                // By accessing .material, we create a new, unique material instance for this object.
                // This ensures we don't accidentally turn the original BingBong blue!
                Material shopkeeperMaterial = renderer.material; 

                // Set the color. "_Color" is the standard property name for the main tint in most Unity shaders.
                // If this doesn't work, we may need to find the correct property name (see below).
                shopkeeperMaterial.SetColor("_Color", Color.blue);
                
                CoinPlugin.Log.LogInfo("Shopkeeper BingBong has been painted blue!");
            }
            else
            {
                CoinPlugin.Log.LogWarning("Could not find a SkinnedMeshRenderer on the BingBong clone to change its color.");
            }
            
            // --- END OF NEW CODE ---

            shopkeeper.AddComponent<ShopkeeperBingBong>();
        }
    }
}