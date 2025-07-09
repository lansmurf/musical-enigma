using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CoinMod
{
    [BepInPlugin("com.yourusername.peakcoinmod", "Peak Coin Mod", "3.0.0")]
    public class CoinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Peak Coin Mod v3.0.0 is loading!");
            
            var harmony = new Harmony("com.yourusername.peakcoinmod.harmony");
            harmony.PatchAll();
            
            Log.LogInfo("Peak Coin Mod is ready for business!");
        }

        // The OnGUI method has been removed as it is no longer used.
    }
}