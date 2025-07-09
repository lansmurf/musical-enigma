using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Luggage))]
    public static class LuggagePatches
    {
        [HarmonyPatch("Interact_CastFinished")]
        [HarmonyPostfix]
        public static void GiveCoinsOnOpenPatch(Character interactor)
        {
            if (interactor != null && interactor.IsLocal)
            {
                // Find the CoinManager on the local player's Player object.
                var coinManager = Player.localPlayer.GetComponent<PlayerCoinManager>();
                if (coinManager != null)
                {
                    int coinsToGive = Random.Range(10, 51);
                    // Call the public method to send the request to the host.
                    coinManager.RequestModifyCoins(coinsToGive);
                    CoinPlugin.Log.LogInfo($"You opened luggage and requested {coinsToGive} coins for the team!");
                }
            }
        }
    }
}