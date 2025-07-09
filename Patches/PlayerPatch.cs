using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class Player_Patch
    {
        private static bool hasInitializedSystems = false;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void OnPlayerAwake(Player __instance)
        {
            // The Player object is persistent, so we add our manager here.
            // This ensures every player, host or client, gets one.
            if (__instance.gameObject.GetComponent<PlayerCoinManager>() == null)
            {
                __instance.gameObject.AddComponent<PlayerCoinManager>();
                CoinPlugin.Log.LogInfo($"Added PlayerCoinManager to player: {__instance.photonView.Owner.NickName}");
            }
            
            // This setup only needs to run ONCE for the local player's entire game session.
            if (__instance.photonView.IsMine && !hasInitializedSystems)
            {
                CoinPlugin.Log.LogInfo("Local player has awoken. Initializing mod UI systems...");
                
                // Create a single, persistent host object for our UI and other managers.
                GameObject modHostObject = new GameObject("PeakCoinMod_Systems");
                Object.DontDestroyOnLoad(modHostObject);

                // Add all our MonoBehaviour managers to this one host object.
                modHostObject.AddComponent<ShopManager>();
                
                // Create and initialize our Coin UI.
                var coinUI = modHostObject.AddComponent<CoinUI>();
                coinUI.Initialize();
                coinUI.SetCanvasParent(GUIManager.instance.hudCanvas.transform);

                hasInitializedSystems = true;
            }
        }
    }
}