using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace CoinMod
{
    [BepInPlugin("com.lansmurf.peakcoinmod", "Peak Coin Mod", "4.1.0")] // Final, working version
    public class CoinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        
        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Peak Coin Mod v4.1.0 is loading!");
            
            var harmony = new Harmony("com.lansmurf.peakcoinmod.harmony");
            harmony.PatchAll();
            
            StartCoroutine(InitializeWhenReady());
            
            Log.LogInfo("Peak Coin Mod is ready and waiting for game to load...");
        }

        private IEnumerator InitializeWhenReady()
        {
            // Wait until the game is fully loaded and the player exists.
            yield return new WaitUntil(() => Player.localPlayer != null);

            Log.LogInfo("Game is ready! Initializing PeakCoinMod systems...");
            
            GameObject modHostObject = new GameObject("PeakCoinMod_Systems");
            DontDestroyOnLoad(modHostObject);

            // Add the components. Their Awake() methods will set their Instances.
            // They will handle creating their own UI when needed.
            modHostObject.AddComponent<ShopManager>();
            modHostObject.AddComponent<CoinUI>();

            // The calls to InitializeUI() have been REMOVED. This fixes the build error.

            // Patch Player.Awake to add our coin manager component.
            var harmony = new Harmony("com.lansmurf.peakcoinmod.player_setup");
            harmony.Patch(
                original: AccessTools.Method(typeof(Player), "Awake"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CoinPlugin), nameof(PlayerAwakePostfix)))
            );
            
            // Manually add to the local player since their Awake() has already run.
            if (Player.localPlayer.GetComponent<PlayerCoinManager>() == null)
            {
                Player.localPlayer.gameObject.AddComponent<PlayerCoinManager>();
            }
        }
        
        public static void PlayerAwakePostfix(Player __instance)
        {
            if (__instance.gameObject.GetComponent<PlayerCoinManager>() == null)
            {
                __instance.gameObject.AddComponent<PlayerCoinManager>();
            }
        }
    }
}