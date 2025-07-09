// ./ShopData.cs

using System.Collections.Generic;

namespace CoinMod
{
    // Our established categories, including "All"
    public enum ItemCategory
    {
        All,
        Food,
        Meds,
        Tools,
        Special
    }
    
    // This helper class remains the same
    public class ShopItemData
    {
        public int Price { get; }
        public ItemCategory Category { get; }

        public ShopItemData(int price, ItemCategory category)
        {
            Price = price;
            Category = category;
        }
    }
    
    // The completely re-categorized item database, using the CORRECT internal prefab names.
    public static class ShopDatabase
    {
        public static readonly int DefaultPrice = 9999;
        public static readonly ItemCategory DefaultCategory = ItemCategory.Special;

        public static readonly Dictionary<string, ShopItemData> ItemData = new Dictionary<string, ShopItemData>
        {
            // --- FOOD --- (Items that primarily restore hunger)
            { "Marshmallow", new ShopItemData(16, ItemCategory.Food) },
            { "Lollipop", new ShopItemData(20, ItemCategory.Food) }, // "Big Lollipop"
            { "Airplane Food", new ShopItemData(11, ItemCategory.Food) },
            { "Granola Bar", new ShopItemData(7, ItemCategory.Food) },
            { "ScoutCookies", new ShopItemData(12, ItemCategory.Food) },
            { "TrailMix", new ShopItemData(5, ItemCategory.Food) },
            
            /*{ "Item_Coconut", new ShopItemData(12, ItemCategory.Food) },
            { "Item_Coconut_half", new ShopItemData(4, ItemCategory.Food) },
            { "Egg", new ShopItemData(5, ItemCategory.Food) },
            { "Item_Honeycomb", new ShopItemData(12, ItemCategory.Food) },
            { "Mushroom Chubby", new ShopItemData(6, ItemCategory.Food) },
            { "Apple Berry Green", new ShopItemData(6, ItemCategory.Food) },
            { "Apple Berry Red", new ShopItemData(6, ItemCategory.Food) },
            { "Apple Berry Yellow", new ShopItemData(6, ItemCategory.Food) },
            { "Kingberry Green", new ShopItemData(10, ItemCategory.Food) },
            { "Kingberry Purple", new ShopItemData(10, ItemCategory.Food) },
            { "Kingberry Yellow", new ShopItemData(10, ItemCategory.Food) },
            { "Winterberry Orange", new ShopItemData(8, ItemCategory.Food) },
            { "Winterberry Yellow", new ShopItemData(50, ItemCategory.Food) },
            { "Clusterberry Black", new ShopItemData(8, ItemCategory.Food) },
            { "Clusterberry Red", new ShopItemData(8, ItemCategory.Food) },
            { "Clusterberry Yellow", new ShopItemData(8, ItemCategory.Food) },
            { "Berrynana Blue", new ShopItemData(8, ItemCategory.Food) },
            { "Berrynana Brown", new ShopItemData(8, ItemCategory.Food) },
            { "Berrynana Pink", new ShopItemData(8, ItemCategory.Food) },
            { "Berrynana Yellow", new ShopItemData(8, ItemCategory.Food) },
*/
            // --- MEDS --- (Items that heal, revive, or cure status effects)
            { "Bandages", new ShopItemData(8, ItemCategory.Meds) },
            { "Heat Pack", new ShopItemData(11, ItemCategory.Meds) },
            { "Antidote", new ShopItemData(23, ItemCategory.Meds) },
            { "FirstAidKit", new ShopItemData(30, ItemCategory.Meds) }, // "Medkit"
            { "Cure-All", new ShopItemData(65, ItemCategory.Meds) },
            { "ScoutEffigy", new ShopItemData(75, ItemCategory.Meds) },
            //{ "HealingPuffShroom", new ShopItemData(15, ItemCategory.Meds) }, // "Remedy Fungus"
            { "Cure-Some", new ShopItemData(25, ItemCategory.Meds) },
            //{ "MedicinalRoot", new ShopItemData(13, ItemCategory.Meds) },
            { "EnergyElixir", new ShopItemData(32, ItemCategory.Meds) },
            { "Energy Drink", new ShopItemData(15, ItemCategory.Meds) },
            { "Sports Drink", new ShopItemData(10, ItemCategory.Meds) }, // Gives stamina, so it's a "med"
            { "Napberry", new ShopItemData(9, ItemCategory.Meds) }, // Cures lethargy

            // --- TOOLS --- (Items for exploration and utility)
            { "Flare", new ShopItemData(6, ItemCategory.Tools) },
            { "Lantern", new ShopItemData(25, ItemCategory.Tools) },
            { "Compass", new ShopItemData(20, ItemCategory.Tools) },
            { "Bugle", new ShopItemData(18, ItemCategory.Tools) },
            { "PortableStovetopItem", new ShopItemData(45, ItemCategory.Tools) }, // "Portable Stove"
            { "RopeShooter", new ShopItemData(38, ItemCategory.Tools) }, // "Rope Cannon"
            { "RopeSpool", new ShopItemData(22, ItemCategory.Tools) },
            { "Anti-Rope Spool", new ShopItemData(22, ItemCategory.Tools) },
            { "RopeShooterAnti", new ShopItemData(42, ItemCategory.Tools) }, // "Anti-Rope Cannon"
            { "ChainShooter", new ShopItemData(40, ItemCategory.Tools) }, // "Chain Cannon"
            { "Piton", new ShopItemData(20, ItemCategory.Tools) },
            { "ShelfShroom", new ShopItemData(5, ItemCategory.Tools) },
            { "Backpack", new ShopItemData(60, ItemCategory.Tools) },
            { "Binoculars", new ShopItemData(15, ItemCategory.Tools) },
            { "Parasol", new ShopItemData(18, ItemCategory.Tools) },
            { "Megaphone", new ShopItemData(12, ItemCategory.Tools) },
            //{ "Flag_Plantable_Seagull", new ShopItemData(10, ItemCategory.Tools) },

            // --- SPECIAL --- (Legendary, cursed, or unique-effect items)
            { "Bugle_Magic", new ShopItemData(88, ItemCategory.Special) }, // "Bugle of Friendship"
            { "Lantern_Faerie", new ShopItemData(70, ItemCategory.Special) }, // "Faerie Lantern"
            { "PandorasBox", new ShopItemData(125, ItemCategory.Special) }, // "Pandora's Lunchbox"

            { "HealingDart Variant", new ShopItemData(35, ItemCategory.Special) }, // "Blowgun"
            { "Cursed Skull", new ShopItemData(100, ItemCategory.Special) },
            { "Pirate Compass", new ShopItemData(80, ItemCategory.Special) },
            { "Bugle_Scoutmaster Variant", new ShopItemData(45, ItemCategory.Special) },
            //{ "Berrynana Peel Blue Variant", new ShopItemData(5, ItemCategory.Special) }, // "Banana Peel"
            { "MagicBean", new ShopItemData(40, ItemCategory.Special) },
            { "Strange Gem", new ShopItemData(50, ItemCategory.Special) },
            //{ "Mushroom Normie Poison", new ShopItemData(5, ItemCategory.Special) },
            //{ "Mushroom Cluster Poison", new ShopItemData(6, ItemCategory.Special) },
            //{ "Mushroom Lace", new ShopItemData(8, ItemCategory.Special) },
            { "BounceShroom", new ShopItemData(8, ItemCategory.Special) },
            { "Warp Compass", new ShopItemData(75, ItemCategory.Special) },
            { "Frisbee", new ShopItemData(3, ItemCategory.Special) }, // Toy/Misc, fits best in Special
            { "BingBong", new ShopItemData(9999, ItemCategory.Special) }, // Not purchasable
        };
    }
}