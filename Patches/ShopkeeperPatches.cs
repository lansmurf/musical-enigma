using HarmonyLib;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(Item))]
    public static class ShopkeeperPatches
    {
        // This patch changes the "Pick up" text to "Shop"
        [HarmonyPatch("GetInteractionText")]
        [HarmonyPrefix]
        public static bool GetInteractionText_Prefix(Item __instance, ref string __result)
        {
            if (__instance.GetComponent<ShopkeeperBingBong>() != null)
            {
                __result = "Shop"; 
                return false;
            }
            return true;
        }

        // --- THIS METHOD IS NOW DISABLED ---
        // Since the shop now requires a campfire for distance checking,
        // this standalone shopkeeper can no longer open the UI correctly.
        /*
        [HarmonyPatch("Interact")]
        [HarmonyPrefix]
        public static bool Interact_Prefix(Item __instance, Character interactor)
        {
            if (__instance.GetComponent<ShopkeeperBingBong>() != null)
            {
                // This line causes the error because we don't have a campfire reference.
                // ShopManager.Instance.OpenShopGUI(); 
                return false;
            }
            return true;
        }
        */
    }
}