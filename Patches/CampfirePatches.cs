using HarmonyLib;
using UnityEngine;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Campfire))]
    public static class CampfirePatches
    {
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
                // Call the new instance method, passing the campfire itself.
                if (ShopManager.Instance != null)
                {
                    ShopManager.Instance.OpenShopGUI(__instance);
                }
                
                return false; 
            }
            return true;
        }
    }
}