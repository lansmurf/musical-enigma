using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace CoinMod
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        private bool isShopOpen = false;
        private Campfire activeCampfire;
        private const float MaxInteractionDistance = 10f;

        private GameObject shopPanel;
        private RectTransform contentRect;
        private TextMeshProUGUI coinText;
        private GameObject itemListingPrefab;
        private static List<Item> allItems = new List<Item>();
        private static bool hasInitializedItems = false;
        
        // You can keep your pricing data here...
        private static readonly int DefaultPrice = 25;
        private static readonly Dictionary<string, int> ItemPrices = new Dictionary<string, int>
        {
            { "Stone", 2 }, { "FireWood", 2 }, { "Marshmallow", 4 }, { "Item_Coconut_half", 4 }, { "Egg", 5 }, { "Frisbee", 3 }, { "Berrynana Peel Blue Variant", 5 }, { "Bugfix", 2 },
            { "Flare", 6 }, { "Bandages", 8 }, { "Airplane Food", 9 }, { "Granola Bar", 9 }, { "Sports Drink", 10 }, { "ScoutCookies", 10 }, { "TrailMix", 11 }, { "Heat Pack", 11 }, { "Item_Coconut", 12 }, { "MedicinalRoot", 13 },
            { "Binoculars", 15 }, { "Parasol", 18 }, { "Compass", 20 }, { "Piton", 20 }, { "RopeSpool", 22 }, { "Lantern", 25 }, { "Energy Drink", 15 }, { "Antidote", 28 }, { "Cure-Some", 25 },
            { "FirstAidKit", 30 }, { "EnergyElixir", 32 }, { "RopeShooter", 38 }, { "ChainShooter", 40 }, { "RopeShooterAnti", 42 }, { "PortableStovetopItem", 45 }, { "Backpack", 60 },
            { "Cure-All", 65 }, { "Lantern_Faerie", 70 }, { "Warp Compass", 75 }, { "Pirate Compass", 80 }, { "Bugle_Magic", 88 }, { "MagicBean", 90 }, { "PandorasBox", 125 }, { "Cursed Skull", 100 }, { "ScoutEffigy", 75 },
            { "Anti-Rope Spool", 22 }, { "Beehive", 15 }, { "NestEgg", 20 }, { "Lollipop", 10 }, { "HealingDart Variant", 35 }, { "BounceShroom", 8 }, { "Bugle", 18 }, { "Mushroom Lace", 8 }, { "Mushroom Normie Poison", 5 }, { "CactusBall", 8 }, { "Mushroom Chubby", 6 }, { "Mushroom Cluster Poison", 6 }, { "Shell Big", 15 }, { "Item_Honeycomb", 12 }, { "Megaphone", 12 }, { "HealingPuffShroom", 15 }, { "Pepper Berry", 8 }, { "Bugle_Scoutmaster Variant", 45 }, { "ShelfShroom", 5 }, { "Strange Gem", 50 }, { "Flag_Plantable_Seagull", 10 }, { "Mushroom Glow", 8 }, { "Wonderberry", 12 },
            { "Clusterberry Black", 8 }, { "Clusterberry Red", 8 }, { "Clusterberry Yellow", 8 }, { "Clusterberry_UNUSED", 8 }, { "Apple Berry Green", 6 }, { "Apple Berry Red", 6 }, { "Apple Berry Yellow", 6 }, { "Kingberry Green", 10 }, { "Kingberry Purple", 10 }, { "Kingberry Yellow", 10 }, { "Berrynana Blue", 8 }, { "Berrynana Brown", 8 }, { "Berrynana Pink", 8 }, { "Berrynana Yellow", 8 }, { "Napberry", 9 }, { "Winterberry Orange", 8 }, { "Winterberry Yellow", 8 },
            { "BingBong", 9999 }, { "Passport", 9999 }, { "Guidebook", 9999 }, { "GuidebookPageScroll Variant", 999 }, { "GuidebookPage_4_BodyHeat Variant", 999 },
        };

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
            
            if (shopPanel == null)
            {
                CreateShopUI();
            }

            activeCampfire = campfire;
            isShopOpen = true;
            shopPanel.SetActive(true);
            
            if (!hasInitializedItems) InitializeItemList();
            PopulateShop();
            
            SetCursorState(true);
        }

        public void CloseShopGUI()
        {
            if (!isShopOpen) return;
            activeCampfire = null;
            isShopOpen = false;
            shopPanel.SetActive(false);
            SetCursorState(false);
        }

        private void PopulateShop()
        {
            foreach (Transform child in contentRect) { Destroy(child.gameObject); }

            int currentCoins = Player.localPlayer?.GetComponent<PlayerCoinManager>()?.SharedCoins ?? 0;
            coinText.text = $"Team Coins: {currentCoins}";

            foreach (var item in allItems)
            {
                if (!ItemPrices.TryGetValue(item.name, out int price)) price = DefaultPrice;
                GameObject newItemEntry = Instantiate(itemListingPrefab, contentRect);
                Image itemIcon = newItemEntry.transform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI itemName = newItemEntry.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
                Button buyButton = newItemEntry.transform.Find("BuyButton").GetComponent<Button>();
                TextMeshProUGUI buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();

                if (item.UIData?.icon != null) {
                    Texture2D tex = item.UIData.icon;
                    itemIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                
                itemName.text = item.UIData?.itemName ?? item.name;
                buyButtonText.text = price < 9000 ? $"Buy ({price})" : "N/A";
                buyButton.interactable = currentCoins >= price && price < 9000;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => TryToBuyItem(item, price));
                newItemEntry.SetActive(true);
            }
        }
        
        private static void InitializeItemList()
        {
            var uniqueItems = new Dictionary<string, Item>();
            var foundItems = Resources.FindObjectsOfTypeAll<Item>();
            foreach (var item in foundItems) {
                if (item != null && item.gameObject.scene.handle == 0 && !string.IsNullOrEmpty(item.UIData?.itemName)) {
                    if (!uniqueItems.ContainsKey(item.UIData.itemName)) { uniqueItems.Add(item.UIData.itemName, item); }
                }
            }
            allItems = uniqueItems.Values.OrderBy(item => item.UIData.itemName).ToList();
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
            PopulateShop();
        }
        
        private static void SetCursorState(bool uiOpen)
        {
            Cursor.visible = uiOpen;
            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
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
            rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = new Vector2(500, 650);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(shopPanel.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "Campfire Shop"; titleText.fontSize = 32; titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1); titleRt.anchoredPosition = new Vector2(0, -20);

            var coinGo = new GameObject("CoinText");
            coinGo.transform.SetParent(shopPanel.transform, false);
            coinText = coinGo.AddComponent<TextMeshProUGUI>();
            coinText.fontSize = 20; coinText.color = Color.yellow;
            coinText.alignment = TextAlignmentOptions.Center;
            var coinRt = coinGo.GetComponent<RectTransform>();
            coinRt.anchorMin = new Vector2(0, 1); coinRt.anchorMax = new Vector2(1, 1);
            coinRt.pivot = new Vector2(0.5f, 1); coinRt.anchoredPosition = new Vector2(0, -60);
            
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(shopPanel.transform, false);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            var scrollImage = scrollGo.AddComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.5f);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0); scrollRt.anchorMax = new Vector2(1, 1);
            scrollRt.pivot = new Vector2(0.5f, 0.5f); scrollRt.anchoredPosition = new Vector2(0, -40);
            scrollRt.sizeDelta = new Vector2(-40, -150);

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;
            viewportGo.AddComponent<Image>();
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
            viewportRt.sizeDelta = Vector2.zero; viewportRt.pivot = new Vector2(0, 1);

            var contentGo = new GameObject("Content");
            contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.transform.SetParent(viewportGo.transform, false);
            contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1); contentRect.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10); vlg.spacing = 5;
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewportRt; scrollRect.content = contentRect;
            
            var closeGo = new GameObject("CloseButton");
            closeGo.transform.SetParent(shopPanel.transform, false);
            var closeButton = closeGo.AddComponent<Button>();
            var closeImage = closeGo.AddComponent<Image>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 0); closeRt.anchorMax = new Vector2(0.5f, 0);
            closeRt.pivot = new Vector2(0.5f, 0); closeRt.sizeDelta = new Vector2(150, 40);
            closeRt.anchoredPosition = new Vector2(0, 20);

            var closeTextGo = new GameObject("Text");
            closeTextGo.transform.SetParent(closeGo.transform, false);
            var closeText = closeTextGo.AddComponent<TextMeshProUGUI>();
            closeText.text = "Close"; closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center; closeText.fontSize = 20;
            closeButton.onClick.AddListener(CloseShopGUI);
            
            CreateItemListingPrefab();
            
            shopPanel.SetActive(false);
        }

        private void CreateItemListingPrefab()
        {
            itemListingPrefab = new GameObject("ItemListingPrefab");
            itemListingPrefab.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            itemListingPrefab.AddComponent<LayoutElement>().minHeight = 50;
            var hlg = itemListingPrefab.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 5, 5); hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            var iconGo = new GameObject("ItemIcon");
            iconGo.transform.SetParent(itemListingPrefab.transform, false);
            iconGo.AddComponent<Image>();
            iconGo.AddComponent<LayoutElement>().preferredWidth = 40;
            var nameGo = new GameObject("ItemName");
            nameGo.transform.SetParent(itemListingPrefab.transform, false);
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.color = Color.white; nameText.fontSize = 18;
            nameGo.AddComponent<LayoutElement>().flexibleWidth = 1;
            var buttonGo = new GameObject("BuyButton");
            buttonGo.transform.SetParent(itemListingPrefab.transform, false);
            buttonGo.AddComponent<Button>();
            buttonGo.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f);
            var buttonLayout = buttonGo.AddComponent<LayoutElement>();
            buttonLayout.minWidth = 100; buttonLayout.preferredWidth = 100;
            var buttonTextGo = new GameObject("Text");
            buttonTextGo.transform.SetParent(buttonGo.transform, false);
            var buttonText = buttonTextGo.AddComponent<TextMeshProUGUI>();
            buttonText.color = Color.white; buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            itemListingPrefab.SetActive(false);
        }
    }
}