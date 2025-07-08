using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CoinMod
{
    [BepInPlugin("com.yourusername.peakcoinmod", "Peak Coin Mod", "1.2.0")]
    public class CoinPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.yourusername.peakcoinmod.harmony");
        internal static ManualLogSource Log;

        public static int PlayerCoins { get; set; } = 1;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Shopkeeper BingBong is preparing his wares...");
            
            // Harmony will find and apply all patch classes in our project
            harmony.PatchAll();
            
            Log.LogInfo("Game Patched. Good luck out there!");
        }

        private void Update()
        {
            if (Character.localCharacter == null || !Input.GetKeyDown(KeyCode.E) || Camera.main == null)
            {
                return;
            }

            // Raycast to see what the player is looking at
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 5f))
            {
                // THE KEY CHANGE: We check for our custom component!
                if (hit.collider.GetComponent<ShopkeeperBingBong>() != null)
                {
                    Log.LogInfo("Player interacted with a Shopkeeper BingBong!");
                    ShopManager.OpenShopGUI();
                }
            }
        }

        private void OnGUI()
        {
            ShopManager.DrawShopGUI();
        }
    }
}