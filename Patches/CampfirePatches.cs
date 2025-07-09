using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Campfire))]
    public static class CampfirePatches
    {
        // --- THIS IS THE NEW, FINAL FIX ---
        // This patch intercepts the game's request for how long to hold the interact button.
        // We force it to 0 for our shop, making it an instant interaction and preventing the hold bar.
        [HarmonyPatch("GetInteractTime")]
        [HarmonyPrefix]
        public static bool SetInstantInteractTime(Campfire __instance, Character interactor, ref float __result)
        {
            if (interactor != null && interactor.data.currentItem == null && __instance.Lit)
            {
                __result = 0f;
                return false; // Skip the original method
            }
            return true; // Let the original method run for all other cases (like cooking)
        }

        // --- Existing Patches (These are all correct) ---

        [HarmonyPatch("IsInteractible")]
        [HarmonyPostfix]
        public static void AllowEmptyHandedInteraction(Campfire __instance, Character interactor, ref bool __result)
        {
            if (__result) return;
            if (__instance.Lit && interactor?.data.currentItem == null)
            {
                __result = true;
            }
        }

        [HarmonyPatch("IsConstantlyInteractable")]
        [HarmonyPostfix]
        public static void AllowEmptyHandedConstantInteraction(Campfire __instance, Character interactor, ref bool __result)
        {
            if (__result) return;
            if (__instance.Lit && interactor?.data.currentItem == null)
            {
                __result = true;
            }
        }

        [HarmonyPatch("GetInteractionText")]
        [HarmonyPostfix]
        public static void OverwriteInteractionText(Campfire __instance, ref string __result)
        {
            if (!__instance.Lit) return;
            Character localPlayer = Character.localCharacter;
            if (localPlayer == null) return;

            if (localPlayer.data.currentItem == null)
            {
                __result = "Shop";
            }
        }

        [HarmonyPatch("Interact")]
        [HarmonyPrefix]
        public static bool HandleShopInteraction(Campfire __instance, Character interactor)
        {
            if (__instance.Lit && interactor.data.currentItem == null)
            {
                if (ShopManager.Instance != null)
                {
                    ShopManager.Instance.OpenShopGUI(__instance);
                }
                else
                {
                    CoinPlugin.Log.LogError("ShopManager.Instance is null! Cannot open shop.");
                }
                
                return false; 
            }
            return true;
        }
    }
}