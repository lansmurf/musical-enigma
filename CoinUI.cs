using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMod
{
    public class CoinUI : MonoBehaviour
    {
        public static CoinUI Instance { get; private set; }
        private TextMeshProUGUI coinText;
        private Canvas canvas;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Create the UI when this component starts. By this time, the game is loaded.
            CreateCoinUICanvas();
        }

        private void CreateCoinUICanvas()
        {
            var canvasGo = new GameObject("PeakCoinMod_CoinCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99; // Render on top, but below the shop
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGo);
            
            GameObject uiObject = new GameObject("CoinCounterText");
            uiObject.transform.SetParent(canvasGo.transform, false);
            
            coinText = uiObject.AddComponent<TextMeshProUGUI>();
            
            // Try to copy style from the game for a native look
            var templateText = GUIManager.instance?.interactNameText;
            if (templateText != null)
            {
                coinText.font = templateText.font;
                coinText.fontMaterial = templateText.fontMaterial;
                coinText.fontSize = 24;
                coinText.color = Color.yellow;
                coinText.alignment = TextAlignmentOptions.Left;
            }
            else // Fallback style
            {
                coinText.fontSize = 24;
                coinText.color = Color.white;
            }
            
            RectTransform rect = uiObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            
            UpdateCoinCount(0); // Set initial text
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