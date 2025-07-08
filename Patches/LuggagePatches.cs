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
            // From your Character.cs file, we know 'IsLocal' is a property on Character.
            if (interactor != null && interactor.IsLocal)
            {
                int coinsToGive = Random.Range(10, 51);
                CoinPlugin.PlayerCoins += coinsToGive;
                CoinPlugin.Log.LogInfo($"You opened luggage and found {coinsToGive} coins!");
                CoinPlugin.Log.LogInfo($"Total coins: {CoinPlugin.PlayerCoins}");
            }
        }
    }
}