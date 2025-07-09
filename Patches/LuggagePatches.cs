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
                // Find the host's coin manager instance.
                if (PlayerCoinManager.HostInstance != null)
                {
                    int coinsToGive = Random.Range(10, 51);
                    // Call the RPC on the host's instance to request a change.
                    PlayerCoinManager.HostInstance.photonView.RPC("RPC_Request_ModifyCoins", PlayerCoinManager.HostInstance.photonView.Owner, coinsToGive);
                    CoinPlugin.Log.LogInfo($"You opened luggage and requested {coinsToGive} coins for the team!");
                }
            }
        }
    }
}