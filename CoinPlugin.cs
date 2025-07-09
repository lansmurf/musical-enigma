using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace CoinMod
{
    [BepInPlugin("com.lansmurf.peakcoinmod", "Peak Coin Mod", "3.2.0")]
    public class CoinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private static bool hasInitialized = false;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Peak Coin Mod v3.2.0 is loading!");
            
            var harmony = new Harmony("com.lansmurf.peakcoinmod.harmony");
            harmony.PatchAll();
            
            // Start a coroutine that will wait for the game to be ready.
            StartCoroutine(InitializeWhenReady());
            
            Log.LogInfo("Peak Coin Mod is ready and waiting for game to load...");
        }

        private IEnumerator InitializeWhenReady()
        {
            // Wait until the local player's character exists and the GUIManager is ready.
            // This is a robust way to ensure the game is fully loaded.
            yield return new WaitUntil(() => Character.localCharacter != null && GUIManager.instance != null);

            // This ensures our setup logic only runs once.
            if (hasInitialized) yield break;
            hasInitialized = true;

            Log.LogInfo("Game is ready! Initializing PeakCoinMod systems...");
            
            // --- Attach PlayerCoinManager to all existing and future players ---
            // This needs a Harmony patch, but we can do it dynamically right here.
            var harmony = new Harmony("com.lansmurf.peakcoinmod.player_setup");
            harmony.Patch(
                original: AccessTools.Method(typeof(Player), "Awake"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CoinPlugin), nameof(PlayerAwakePostfix)))
            );

            // The local player might already exist, so we need to manually add the component to them.
            if (Player.localPlayer != null && Player.localPlayer.GetComponent<PlayerCoinManager>() == null)
            {
                Player.localPlayer.gameObject.AddComponent<PlayerCoinManager>();
                Log.LogInfo($"Added PlayerCoinManager to already-existing local player.");
            }
            
            // --- Create the persistent UI and Shop Manager ---
            GameObject modHostObject = new GameObject("PeakCoinMod_Systems");
            DontDestroyOnLoad(modHostObject);

            modHostObject.AddComponent<ShopManager>();
            
            var coinUI = modHostObject.AddComponent<CoinUI>();
            coinUI.Initialize();
            coinUI.SetCanvasParent(GUIManager.instance.hudCanvas.transform);
        }

        // This is the method our dynamic Harmony patch will call.
        public static void PlayerAwakePostfix(Player __instance)
        {
            if (__instance.gameObject.GetComponent<PlayerCoinManager>() == null)
            {
                __instance.gameObject.AddComponent<PlayerCoinManager>();
                Log.LogInfo($"Added PlayerCoinManager to player: {__instance.photonView.Owner.NickName}");
            }
        }
    }
}