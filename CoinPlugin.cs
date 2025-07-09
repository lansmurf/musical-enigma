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
            
            StartCoroutine(InitializeWhenReady());
            
            Log.LogInfo("Peak Coin Mod is ready and waiting for game to load...");
        }

        private IEnumerator InitializeWhenReady()
        {
            yield return new WaitUntil(() => Character.localCharacter != null && GUIManager.instance != null);

            if (hasInitialized) yield break;
            hasInitialized = true;

            Log.LogInfo("Game is ready! Initializing PeakCoinMod systems...");
            
            var harmony = new Harmony("com.lansmurf.peakcoinmod.player_setup");
            harmony.Patch(
                original: AccessTools.Method(typeof(Player), "Awake"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CoinPlugin), nameof(PlayerAwakePostfix)))
            );

            if (Player.localPlayer != null && Player.localPlayer.GetComponent<PlayerCoinManager>() == null)
            {
                Player.localPlayer.gameObject.AddComponent<PlayerCoinManager>();
                Log.LogInfo($"Added PlayerCoinManager to already-existing local player.");
            }
            
            GameObject modHostObject = new GameObject("PeakCoinMod_Systems");
            DontDestroyOnLoad(modHostObject);

            modHostObject.AddComponent<ShopManager>();
            
            var coinUI = modHostObject.AddComponent<CoinUI>();
            coinUI.Initialize();
            coinUI.SetCanvasParent(GUIManager.instance.hudCanvas.transform);
            
            // --- THE FIX FOR UI VISIBILITY ---
            modHostObject.SetActive(true);
        }

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