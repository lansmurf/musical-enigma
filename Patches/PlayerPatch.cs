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
            if (__instance.gameObject.GetComponent<PlayerCoinManager>() == null)
            {
                __instance.gameObject.AddComponent<PlayerCoinManager>();
            }
            
            // This setup only needs to run ONCE for the local player's entire game session.
            if (__instance.photonView.IsMine && !hasInitializedSystems)
            {
                CoinPlugin.Log.LogInfo("Local player awakened for the first time. Initializing mod systems...");
                
                // --- Create a single, persistent host object for our UI managers ---
                GameObject modHostObject = new GameObject("PeakCoinMod_Systems");
                Object.DontDestroyOnLoad(modHostObject);

                // Add all our MonoBehaviour managers to this one host object.
                // The managers will now control their own lifecycle.
                modHostObject.AddComponent<ShopManager>();
                
                if (CoinUI.Instance == null)
                {
                   var coinUI = modHostObject.AddComponent<CoinUI>();
                   coinUI.Initialize();
                   coinUI.SetCanvasParent(GUIManager.instance.hudCanvas.transform);
                }

                hasInitializedSystems = true;
            }
        }
    }
}