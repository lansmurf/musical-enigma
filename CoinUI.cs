using TMPro;
using UnityEngine;

namespace CoinMod
{
    public class CoinUI : MonoBehaviour
    {
        public static CoinUI Instance { get; private set; }
        
        private TextMeshProUGUI coinText;
        private int lastDisplayedCoins = -1;

        public void Initialize()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            coinText = gameObject.AddComponent<TextMeshProUGUI>();
            
            // --- THE NEW, IMPROVED STYLING ---
            var templateText = GUIManager.instance.interactNameText;
            if (templateText != null)
            {
                CoinPlugin.Log.LogInfo("Applying UI style from game's interact text.");
                // Copy the essential properties for a perfect match
                coinText.font = templateText.font;
                coinText.fontMaterial = templateText.fontMaterial; // This is the key for outlines/effects
                coinText.fontSize = templateText.fontSize;
                coinText.color = templateText.color; // Use the game's exact text color
                coinText.alignment = TextAlignmentOptions.Left;
                
                // We no longer need to manually set outline properties,
                // as they are inherited from the fontMaterial.
            }
            else
            {
                // Fallback to basic styling if the template isn't found
                CoinPlugin.Log.LogWarning("Could not find UI template, using basic style.");
                coinText.fontSize = 24;
                coinText.color = Color.white;
            }
        }
        
        public void SetCanvasParent(Transform canvasParent)
        {
            transform.SetParent(canvasParent, false);

            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
        }

        private void Update()
        {
            if (PlayerCoinManager.HostInstance == null) return;

            int currentCoins = PlayerCoinManager.HostInstance.SharedCoins;
            if (currentCoins != lastDisplayedCoins)
            {
                UpdateCoinCount(currentCoins);
                lastDisplayedCoins = currentCoins;
            }
        }

        public void UpdateCoinCount(int newAmount)
        {
            if (coinText != null)
            {
                coinText.text = $"Coins: {newAmount}"; 
            }
        }
    }
}