// ./ShopManager.cs

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

namespace CoinMod
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        public bool isShopOpen { get; private set; } = false;
        private Campfire activeCampfire;
        private const float MaxInteractionDistance = 10f;

        // UI References
        private GameObject shopPanel;
        private RectTransform itemContentRect;
        private TextMeshProUGUI coinText;
        private GameObject itemListingPrefab;
        private RectTransform categoryButtonContainer;
        private GameObject categoryButtonPrefab;

        // Data & State
        private static List<Item> allItems = new List<Item>();
        private static bool hasInitializedItems = false;
        private ItemCategory currentCategory = ItemCategory.All;
        private Dictionary<ItemCategory, Button> categoryButtons = new Dictionary<ItemCategory, Button>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (isShopOpen && (activeCampfire == null || Character.localCharacter == null || Vector3.Distance(Character.localCharacter.Center, activeCampfire.transform.position) > MaxInteractionDistance))
            {
                CloseShopGUI();
            }
        }
        
        public void OpenShopGUI(Campfire campfire)
        {
            if (isShopOpen) return;
            if (shopPanel == null) CreateShopUI();
            
            activeCampfire = campfire;
            isShopOpen = true;
            shopPanel.SetActive(true);
            
            if (!hasInitializedItems) InitializeItemList();
            
            SetCategory(ItemCategory.All);
        }

        public void CloseShopGUI()
        {
            if (!isShopOpen) return;
            activeCampfire = null;
            isShopOpen = false;
            shopPanel.SetActive(false);
        }

        private void SetCategory(ItemCategory category)
        {
            currentCategory = category;
            
            foreach (var catButton in categoryButtons)
            {
                var image = catButton.Value.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (catButton.Key == currentCategory)
                        ? new Color(0.3f, 0.5f, 0.8f) // Active/Selected color
                        : new Color(0.15f, 0.25f, 0.4f); // Normal color
                }
            }
            
            PopulateItemGrid();
        }

        private void PopulateItemGrid()
        {
            foreach (Transform child in itemContentRect) { Destroy(child.gameObject); }

            int currentCoins = Player.localPlayer?.GetComponent<PlayerCoinManager>()?.SharedCoins ?? 0;
            coinText.text = $"Team Coins: {currentCoins}";

            var itemsToDisplay = allItems
                .Select(item => {
                    ShopDatabase.ItemData.TryGetValue(item.name, out var data);
                    return new {
                        Item = item,
                        Price = data?.Price ?? ShopDatabase.DefaultPrice,
                        Category = data?.Category ?? ItemCategory.Special
                    };
                })
                .Where(x => ShopDatabase.ItemData.ContainsKey(x.Item.name))
                .Where(x => currentCategory == ItemCategory.All || x.Category == currentCategory)
                .OrderBy(x => x.Price);

            foreach (var itemData in itemsToDisplay)
            {
                GameObject newItemEntry = Instantiate(itemListingPrefab, itemContentRect);
                Image itemIcon = newItemEntry.transform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI itemName = newItemEntry.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
                Button buyButton = newItemEntry.transform.Find("BuyButton").GetComponent<Button>();
                TextMeshProUGUI buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();

                if (itemData.Item.UIData?.icon != null) {
                    Texture2D tex = itemData.Item.UIData.icon;
                    itemIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                
                itemName.text = itemData.Item.UIData?.itemName ?? itemData.Item.name;
                buyButtonText.text = $"Buy ({itemData.Price})";
                buyButton.interactable = currentCoins >= itemData.Price;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => TryToBuyItem(itemData.Item, itemData.Price));
                newItemEntry.SetActive(true);
            }
        }
        
        private static void InitializeItemList()
        {
            var uniqueItems = new Dictionary<string, Item>();
            var foundItems = Resources.FindObjectsOfTypeAll<Item>();
            foreach (var item in foundItems) {
                if (item != null && item.gameObject.scene.handle == 0 && !string.IsNullOrEmpty(item.UIData?.itemName) && ShopDatabase.ItemData.ContainsKey(item.name)) {
                    if (!uniqueItems.ContainsKey(item.UIData.itemName)) {
                        uniqueItems.Add(item.UIData.itemName, item);
                    }
                }
            }
            allItems = uniqueItems.Values.ToList();
            hasInitializedItems = true;
        }

        private void TryToBuyItem(Item itemPrefab, int price)
        {
            var coinManager = Player.localPlayer?.GetComponent<PlayerCoinManager>();
            if (coinManager == null || coinManager.SharedCoins < price) return;
            coinManager.RequestModifyCoins(-price);
            if (Character.localCharacter != null) {
                Vector3 spawnPos = Character.localCharacter.Center + Character.localCharacter.transform.forward * 1.5f + Vector3.up * 0.5f;
                PhotonNetwork.Instantiate("0_Items/" + itemPrefab.name, spawnPos, Quaternion.identity);
            }
            PopulateItemGrid(); 
        }

        private void CreateShopUI()
        {
            var canvasGo = new GameObject("PeakCoinMod_ShopCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGo);
            
            shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(canvasGo.transform, false);
            var panelImage = shopPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            var rt = shopPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1000, 750);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(shopPanel.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "Campfire Shop"; titleText.fontSize = 32; titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1); titleRt.anchoredPosition = new Vector2(0, -20);

            coinText = new GameObject("CoinText").AddComponent<TextMeshProUGUI>();
            coinText.transform.SetParent(shopPanel.transform, false);
            coinText.fontSize = 20; coinText.color = Color.yellow;
            coinText.alignment = TextAlignmentOptions.Center;
            var coinRt = coinText.GetComponent<RectTransform>();
            coinRt.anchorMin = new Vector2(0, 1); coinRt.anchorMax = new Vector2(1, 1);
            coinRt.pivot = new Vector2(0.5f, 1); coinRt.anchoredPosition = new Vector2(0, -60);
            
            var categoryPanel = new GameObject("CategoryPanel", typeof(RectTransform));
            categoryPanel.transform.SetParent(shopPanel.transform, false);
            categoryPanel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            var categoryRt = categoryPanel.GetComponent<RectTransform>();
            
            // --- THIS IS THE FIX for the panel layout ---
            // We anchor the category panel to the left side of the main shop panel.
            categoryRt.anchorMin = new Vector2(0, 0);
            categoryRt.anchorMax = new Vector2(0, 1);
            categoryRt.pivot = new Vector2(0, 1); // Top-left pivot
            // Set position and size using offsets from the edges.
            // Left: 20px, Right: 1000 - 220 = 780px, Top: 100px, Bottom: 80px
            categoryRt.offsetMin = new Vector2(20, 80);  // Left, Bottom
            categoryRt.offsetMax = new Vector2(220, -100); // -Right, -Top

            var containerGo = new GameObject("CategoryButtonContainer", typeof(RectTransform));
            containerGo.transform.SetParent(categoryPanel.transform, false);
            categoryButtonContainer = containerGo.GetComponent<RectTransform>();
            // Make the button container fill the category panel
            categoryButtonContainer.anchorMin = Vector2.zero;
            categoryButtonContainer.anchorMax = Vector2.one;
            categoryButtonContainer.sizeDelta = Vector2.zero;
            var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10); vlg.spacing = 10;
            vlg.childForceExpandHeight = false;

            var itemPanel = new GameObject("ItemPanel", typeof(RectTransform));
            itemPanel.transform.SetParent(shopPanel.transform, false);
            // Anchor the item panel to stretch in the remaining space.
            var itemPanelRt = itemPanel.GetComponent<RectTransform>();
            itemPanelRt.anchorMin = new Vector2(0, 0);
            itemPanelRt.anchorMax = new Vector2(1, 1);
            // Left: 240px (220 from cat panel + 20 gap), Right: 20px, Top: 100px, Bottom: 80px
            itemPanelRt.offsetMin = new Vector2(240, 80);
            itemPanelRt.offsetMax = new Vector2(-20, -100);

            var scrollRect = itemPanel.AddComponent<ScrollRect>();
            itemPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            scrollRect.horizontal = false; scrollRect.vertical = true;
            
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(itemPanel.transform, false);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;
            viewportGo.AddComponent<Image>();
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
            viewportRt.sizeDelta = Vector2.zero; viewportRt.pivot = new Vector2(0, 1);

            var contentGo = new GameObject("ItemContent", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            itemContentRect = contentGo.GetComponent<RectTransform>();
            itemContentRect.anchorMin = new Vector2(0, 1); itemContentRect.anchorMax = new Vector2(1, 1);
            itemContentRect.pivot = new Vector2(0.5f, 1);
            
            var gridLayout = contentGo.AddComponent<GridLayoutGroup>();
            gridLayout.padding = new RectOffset(15, 15, 15, 15);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.cellSize = new Vector2(150, 180);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewportRt; scrollRect.content = itemContentRect;

            var closeButton = new GameObject("CloseButton", typeof(RectTransform)).AddComponent<Button>();
            closeButton.transform.SetParent(shopPanel.transform, false);
            closeButton.gameObject.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
            var closeRt = closeButton.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 0); closeRt.anchorMax = new Vector2(0.5f, 0);
            closeRt.pivot = new Vector2(0.5f, 0); closeRt.sizeDelta = new Vector2(150, 40);
            closeRt.anchoredPosition = new Vector2(0, 20);

            var closeText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            closeText.transform.SetParent(closeButton.transform, false);
            closeText.text = "Close"; closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.fontSize = 20;
            closeButton.onClick.AddListener(CloseShopGUI);
            
            CreateItemListingPrefab();
            CreateCategoryButtonPrefab();
            
            PopulateCategoryTabs();
            
            shopPanel.SetActive(false);
        }
        
        private void PopulateCategoryTabs()
        {
            foreach (Transform child in categoryButtonContainer) { Destroy(child.gameObject); }
            categoryButtons.Clear();

            foreach (ItemCategory categoryValue in Enum.GetValues(typeof(ItemCategory)))
            {
                ItemCategory localCategory = categoryValue;
                GameObject buttonGo = Instantiate(categoryButtonPrefab, categoryButtonContainer);
                var buttonText = buttonGo.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = localCategory.ToString();
                
                var button = buttonGo.GetComponent<Button>();
                button.onClick.AddListener(() => SetCategory(localCategory));
                
                categoryButtons[localCategory] = button;
                
                buttonGo.SetActive(true);
            }
        }

        private void CreateItemListingPrefab()
        {
            itemListingPrefab = new GameObject("ItemListingPrefab");
            itemListingPrefab.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            var vlg = itemListingPrefab.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var iconGo = new GameObject("ItemIcon", typeof(RectTransform));
            iconGo.transform.SetParent(itemListingPrefab.transform, false);
            iconGo.AddComponent<Image>();
            var iconLayout = iconGo.AddComponent<LayoutElement>();
            iconLayout.minHeight = 80;
            iconLayout.preferredHeight = 80;

            var nameGo = new GameObject("ItemName", typeof(RectTransform));
            nameGo.transform.SetParent(itemListingPrefab.transform, false);
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.color = Color.white;
            nameText.fontSize = 16;
            nameText.alignment = TextAlignmentOptions.Center;
            nameGo.AddComponent<LayoutElement>().minHeight = 40;

            var buttonGo = new GameObject("BuyButton", typeof(RectTransform));
            buttonGo.transform.SetParent(itemListingPrefab.transform, false);
            buttonGo.AddComponent<Button>();
            buttonGo.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f);
            var buttonLayout = buttonGo.AddComponent<LayoutElement>();
            buttonLayout.minHeight = 30;
            buttonLayout.preferredHeight = 30;

            var buttonTextGo = new GameObject("Text", typeof(RectTransform));
            buttonTextGo.transform.SetParent(buttonGo.transform, false);
            var buttonText = buttonTextGo.AddComponent<TextMeshProUGUI>();
            buttonText.color = Color.white;
            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            itemListingPrefab.SetActive(false);
        }

        private void CreateCategoryButtonPrefab()
        {
            categoryButtonPrefab = new GameObject("CategoryButtonPrefab");
            var image = categoryButtonPrefab.AddComponent<Image>();
            image.color = new Color(0.15f, 0.25f, 0.4f);
            categoryButtonPrefab.AddComponent<LayoutElement>().minHeight = 50;
            
            var button = categoryButtonPrefab.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.4f, 0.6f);
            colors.pressedColor = new Color(0.1f, 0.2f, 0.3f);
            button.colors = colors;
            
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(categoryButtonPrefab.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.color = Color.white;
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Center;
            
            categoryButtonPrefab.SetActive(false);
        }
    }
}