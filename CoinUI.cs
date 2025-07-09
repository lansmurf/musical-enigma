using TMPro;
using UnityEngine;

namespace CoinMod
{
    public class CoinUI : MonoBehaviour
    {
        public static CoinUI Instance { get; private set; }
        
        private TextMeshProUGUI coinText;

        public void Initialize()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            coinText = gameObject.AddComponent<TextMeshProUGUI>();
            
            // Apply styling from the game's UI for a native look.
            var templateText = GUIManager.instance.interactNameText;
            if (templateText != null)
            {
                CoinPlugin.Log.LogInfo("Applying UI style from game's interact text.");
                coinText.font = templateText.font;
                coinText.fontMaterial = templateText.fontMaterial;
                coinText.fontSize = templateText.fontSize;
                coinText.color = Color.yellow; // We can override color if we want.
                coinText.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                CoinPlugin.Log.LogWarning("Could not find UI template, using basic style.");
                coinText.fontSize = 24;
                coinText.color = Color.white;
            }
        }
        
        // This method positions our UI element on the main game canvas.
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

        // This is the only method needed to update the display.
        // It is called by PlayerCoinManager when it receives an update from the host.
        public void UpdateCoinCount(int newAmount)
        {
            if (coinText != null)
            {
                coinText.text = $"Coins: {newAmount}"; 
            }
        }
    }
}