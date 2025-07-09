// ./Patches/GUIManagerPatches.cs

using HarmonyLib;
using System.Reflection;

namespace CoinMod.Patches
{
    [HarmonyPatch(typeof(GUIManager))]
    public static class GUIManagerPatches
    {
        private static FieldInfo _windowShowingCursorField = AccessTools.Field(typeof(GUIManager), "<windowShowingCursor>k__BackingField");
        private static FieldInfo _windowBlockingInputField = AccessTools.Field(typeof(GUIManager), "<windowBlockingInput>k__BackingField");

        [HarmonyPatch("UpdateWindowStatus")]
        [HarmonyPostfix]
        public static void ForceCursorStateForShop(GUIManager __instance)
        {
            // --- THIS IS THE FIX ---
            // First, check if our ShopManager even exists yet. If it doesn't (like in the main menu),
            // do nothing. This prevents the startup crash.
            if (ShopManager.Instance == null)
            {
                return;
            }

            if (_windowShowingCursorField == null || _windowBlockingInputField == null)
            {
                if (ShopManager.Instance.isShopOpen)
                {
                     CoinPlugin.Log.LogError("Could not find GUIManager fields! Cursor will not work.");
                }
                return;
            }
            
            if (ShopManager.Instance.isShopOpen)
            {
                _windowShowingCursorField.SetValue(__instance, true);
                _windowBlockingInputField.SetValue(__instance, true);
            }
        }
    }
}