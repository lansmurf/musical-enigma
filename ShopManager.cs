using UnityEngine;
using Photon.Pun;
using System.Linq;
using Zorro.Core;

namespace CoinMod
{
    public static class ShopManager
    {
        private static bool isShopOpen = false;
        private static Rect windowRect = new Rect(20, 20, 400, 300);

        /// <summary>
        /// A dedicated function to OPEN the shop UI. Called by our patches.
        /// </summary>
        public static void OpenShopGUI()
        {
            if (isShopOpen) return; // Don't do anything if it's already open
            isShopOpen = true;
            SetCursorState(true);
        }

        /// <summary>
        /// A dedicated function to CLOSE the shop UI. Called by the UI button.
        /// </summary>
        public static void CloseShopGUI()
        {
            if (!isShopOpen) return; // Don't do anything if it's already closed
            isShopOpen = false;
            SetCursorState(false);
        }

        /// <summary>
        /// Helper function to lock or unlock the mouse cursor.
        /// </summary>
        private static void SetCursorState(bool uiOpen)
        {
            Cursor.visible = uiOpen;
            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }

        /// <summary>
        /// This is called by the main plugin's OnGUI every frame.
        /// It draws the window only if isShopOpen is true.
        /// </summary>
        public static void DrawShopGUI()
        {
            if (!isShopOpen) return;
            windowRect = GUI.Window(0, windowRect, ShopWindowFunction, "Coin Shop");
        }

        /// <summary>
        /// This function defines the actual content of our shop window.
        /// </summary>
        private static void ShopWindowFunction(int windowID)
        {
            GUILayout.BeginVertical();
            
            GUILayout.Label($"Your Coins: {CoinPlugin.PlayerCoins}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
            GUILayout.Space(10);

            // This new method finds the item first, then calls the drawing function.
            DrawItemEntry("Flare", 1);
            DrawItemEntry("ClimbingAxe", 1);
            DrawItemEntry("Rope", 1);
            DrawItemEntry("Food", 1);
            DrawItemEntry("Lantern", 1); 

            if (GUILayout.Button("Close"))
            {
                CloseShopGUI();
            }
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        /// <summary>
        /// A helper function to find an item and then call the drawing function.
        /// </summary>
        private static void DrawItemEntry(string itemName, int price)
        {
            Item itemPrefab = FindItemPrefabByName(itemName);
            if (itemPrefab != null)
            {
                DrawBuyButton(itemPrefab, price);
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button($"'{itemName}' - Not Found");
                GUI.enabled = true;
            }
        }
        
        /// <summary>
        /// Draws a single row in our shop, with an icon.
        /// </summary>
        private static void DrawBuyButton(Item itemPrefab, int price)
        {
            GUILayout.BeginHorizontal();

            if (itemPrefab.UIData != null && itemPrefab.UIData.icon != null)
            {
                GUILayout.Box(itemPrefab.UIData.icon, GUILayout.Width(40), GUILayout.Height(40));
            }
            else
            {
                GUILayout.Space(44); 
            }

            GUI.enabled = CoinPlugin.PlayerCoins >= price;
            if (GUILayout.Button($"{itemPrefab.UIData.itemName} - {price} Coins"))
            {
                TryToBuyItem(itemPrefab, price);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        /// <summary>
        /// The logic for when a player clicks a buy button.
        /// </summary>
        private static void TryToBuyItem(Item itemPrefab, int price)
        {
            CoinPlugin.PlayerCoins -= price;
            CoinPlugin.Log.LogInfo($"Bought {itemPrefab.UIData.itemName} for {price}. Remaining coins: {CoinPlugin.PlayerCoins}");

            if(Character.localCharacter != null)
            {
                Vector3 spawnPos = Character.localCharacter.Center + Character.localCharacter.transform.forward * 1.5f + Vector3.up * 0.5f;
                PhotonNetwork.Instantiate("0_Items/" + itemPrefab.name, spawnPos, Quaternion.identity);
                CoinPlugin.Log.LogInfo($"Spawned {itemPrefab.name} in the world.");
            }
            
            CloseShopGUI();
        }

        /// <summary>
        /// Searches the game's ItemDatabase for an item prefab by its internal name.
        /// </summary>
        private static Item FindItemPrefabByName(string name)
        {
            ItemDatabase db = SingletonAsset<ItemDatabase>.Instance;
            if (db == null)
            {
                CoinPlugin.Log.LogError("ItemDatabase instance is null!");
                return null;
            }
            return db.itemLookup.Values.FirstOrDefault(item => item.name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}