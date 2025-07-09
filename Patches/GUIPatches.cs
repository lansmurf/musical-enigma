// ./Patches/GUIManagerPatches.cs

using HarmonyLib;
using System.Reflection;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(GUIManager))]
    public static class GUIManagerPatches
    {
        // Use Harmony's AccessTools to find the private backing fields for the properties.
        // The compiler generates these fields with "weird" names.
        private static FieldInfo _windowShowingCursorField = AccessTools.Field(typeof(GUIManager), "<windowShowingCursor>k__BackingField");
        private static FieldInfo _windowBlockingInputField = AccessTools.Field(typeof(GUIManager), "<windowBlockingInput>k__BackingField");

        [HarmonyPatch("UpdateWindowStatus")]
        [HarmonyPostfix]
        public static void ForceCursorStateForShop(GUIManager __instance)
        {
            // First, check that our reflection was successful to prevent errors.
            if (_windowShowingCursorField == null || _windowBlockingInputField == null)
            {
                if (ShopManager.Instance != null && ShopManager.Instance.isShopOpen)
                {
                     CoinPlugin.Log.LogError("Could not find GUIManager fields! Cursor will not work.");
                }
                return;
            }

            // If our shop is open...
            if (ShopManager.Instance != null && ShopManager.Instance.isShopOpen)
            {
                // ...force the game to believe a window is showing a cursor and blocking input.
                // We are writing directly to the private fields, bypassing the read-only property.
                _windowShowingCursorField.SetValue(__instance, true);
                _windowBlockingInputField.SetValue(__instance, true);
            }
        }
    }
}