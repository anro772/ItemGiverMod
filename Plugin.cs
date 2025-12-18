using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using UnityEngine;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using System;
using System.Linq;
using Il2CppInterop.Runtime.Injection;

namespace ItemGiverMod
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BasePlugin
    {
        public const string
            MODNAME = "ItemGiver",
            AUTHOR = "anro",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "2.0.2";

        private static ManualLogSource log;

        // UI State
        private static bool showMenu = false;
        private static Vector2 scrollPosition = Vector2.zero;
        private static Rect windowRect = new Rect(50, 50, 750, 700);
        private static string searchText = "";

        // UI Styling
        private static Texture2D backgroundTexture;
        private static GUIStyle windowStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle searchBoxStyle;
        private static GUIStyle bigButtonStyle;
        private static bool stylesInitialized = false;

        // All items from the EItem enum
        private static EItem[] allItems;
        private static string[] itemNames;

        public Plugin()
        {
            log = Log;
        }

        public override void Load()
        {
            log.LogInfo($"Loading {MODNAME} v{VERSION} by {AUTHOR}");

            allItems = (EItem[])Enum.GetValues(typeof(EItem));
            itemNames = Enum.GetNames(typeof(EItem));

            ClassInjector.RegisterTypeInIl2Cpp<ItemGiverUI>();
            AddComponent<ItemGiverUI>();

            log.LogInfo($"Loaded {allItems.Length} items");
            log.LogInfo($"{MODNAME} loaded! Press F3 to open the item menu.");
        }

        public class ItemGiverUI : MonoBehaviour
        {
            public ItemGiverUI(IntPtr ptr) : base(ptr) { }

            private void Start()
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
                log.LogInfo("ItemGiver UI initialized!");
            }

            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    showMenu = !showMenu;
                    if (showMenu)
                    {
                        searchText = ""; // Clear search when opening
                    }
                    log.LogInfo($"Item menu {(showMenu ? "opened" : "closed")}");
                }

                // Handle search input when menu is open
                if (showMenu)
                {
                    HandleSearchInput();
                }
            }

            private void OnGUI()
            {
                if (!showMenu) return;

                var gameManager = GameManager.Instance;
                if (gameManager == null) return;

                var playerInventory = gameManager.GetPlayerInventory();
                if (playerInventory == null) return;

                // Check if texture was destroyed (happens when transitioning between runs)
                // If destroyed, reinitialize styles
                if (stylesInitialized && backgroundTexture == null)
                {
                    stylesInitialized = false;
                }

                // Initialize styles when needed
                if (!stylesInitialized)
                {
                    InitializeStyles();
                }

                try
                {
                    windowRect = GUI.Window(12345, windowRect, (GUI.WindowFunction)DrawWindow, "Item Giver v2.0 - Press F3 to close", windowStyle);
                }
                catch (Exception ex)
                {
                    log.LogError($"GUI.Window failed: {ex.Message}");
                }
            }
        }

        private static void HandleSearchInput()
        {
            // Capture keyboard input without using GUI.TextField
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Backspace
                {
                    if (searchText.Length > 0)
                    {
                        searchText = searchText.Substring(0, searchText.Length - 1);
                    }
                }
                else if (c == '\n' || c == '\r') // Enter - do nothing
                {
                    // Ignore enter key
                }
                else if (char.IsLetterOrDigit(c) || c == ' ')
                {
                    searchText += c;
                }
            }
        }

        private static void InitializeStyles()
        {
            // Create solid background texture
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 0.95f));
            backgroundTexture.Apply();

            // Window style
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = backgroundTexture;
            windowStyle.onNormal.background = backgroundTexture;
            windowStyle.normal.textColor = Color.white;
            windowStyle.fontSize = 14;

            // Label style
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 13;

            // Button style
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 12;

            // Big button style for bulk actions
            bigButtonStyle = new GUIStyle(GUI.skin.button);
            bigButtonStyle.normal.textColor = Color.white;
            bigButtonStyle.fontSize = 11;
            bigButtonStyle.fontStyle = FontStyle.Bold;
            bigButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Search box style (using GUI.Box to fake a text field)
            searchBoxStyle = new GUIStyle(GUI.skin.box);
            searchBoxStyle.normal.textColor = Color.white;
            searchBoxStyle.fontSize = 13;

            stylesInitialized = true;
        }

        private static void DrawWindow(int windowID)
        {
            try
            {
                // Make window draggable
                GUI.DragWindow(new Rect(0, 0, 10000, 20));

                int yOffset = 25;

                // === BULK ACTIONS SECTION ===
                GUI.Label(new Rect(10, yOffset, 730, 20), "🎁 Bulk Actions:", labelStyle);
                yOffset += 22;

                // Row 1: All items + Legendary + Epic
                if (GUI.Button(new Rect(10, yOffset, 140, 28), "Give All Items x1", bigButtonStyle))
                {
                    GiveAllItems();
                }
                if (GUI.Button(new Rect(160, yOffset, 140, 28), "All Legendary x1", bigButtonStyle))
                {
                    GiveItemsByRarity(EItemRarity.Legendary);
                }
                if (GUI.Button(new Rect(310, yOffset, 140, 28), "All Epic x1", bigButtonStyle))
                {
                    GiveItemsByRarity(EItemRarity.Epic);
                }
                if (GUI.Button(new Rect(460, yOffset, 140, 28), "All Rare x1", bigButtonStyle))
                {
                    GiveItemsByRarity(EItemRarity.Rare);
                }
                if (GUI.Button(new Rect(610, yOffset, 130, 28), "All Common x1", bigButtonStyle))
                {
                    GiveItemsByRarity(EItemRarity.Common);
                }
                yOffset += 35;

                // Draw separator
                GUI.Box(new Rect(10, yOffset, 730, 2), "");
                yOffset += 7;

                // === SEARCH SECTION ===
                GUI.Label(new Rect(10, yOffset, 60, 25), "Search:", labelStyle);

                // Draw search box background
                GUI.Box(new Rect(75, yOffset, 540, 25), "");

                // Display search text with cursor
                string displayText = string.IsNullOrEmpty(searchText) ? "Type to search..." : searchText;
                Color textColor = string.IsNullOrEmpty(searchText) ? new Color(0.6f, 0.6f, 0.6f) : Color.white;

                var searchTextStyle = new GUIStyle(labelStyle);
                searchTextStyle.normal.textColor = textColor;
                GUI.Label(new Rect(80, yOffset + 1, 530, 23), displayText + "_", searchTextStyle);

                // Clear button
                if (GUI.Button(new Rect(625, yOffset, 80, 25), "Clear", buttonStyle))
                {
                    searchText = "";
                }
                yOffset += 30;

                // Get filtered items
                var filteredItems = GetFilteredItems();

                // Info label
                GUI.Label(new Rect(10, yOffset, 730, 20),
                    $"Found {filteredItems.Length} of {allItems.Length} items - Type letters to filter",
                    labelStyle);
                yOffset += 22;

                // Draw separator line
                GUI.Box(new Rect(10, yOffset, 730, 2), "");
                yOffset += 5;

                // === SCROLLABLE ITEM LIST ===
                Rect scrollViewRect = new Rect(10, yOffset, 730, 415);
                Rect scrollContentRect = new Rect(0, 0, 700, filteredItems.Length * 30);

                scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContentRect, false, true);

                // Draw filtered items
                int yPos = 0;
                for (int i = 0; i < filteredItems.Length; i++)
                {
                    int itemIndex = Array.IndexOf(allItems, filteredItems[i]);

                    // Alternating row background
                    if (i % 2 == 0)
                    {
                        GUI.Box(new Rect(0, yPos, 700, 28), "", GUI.skin.box);
                    }

                    // Get item rarity for color coding
                    string itemNameWithRarity = GetItemNameWithRarity(filteredItems[i], itemIndex);

                    var itemLabelStyle = new GUIStyle(labelStyle);
                    itemLabelStyle.normal.textColor = GetRarityColor(filteredItems[i]);
                    GUI.Label(new Rect(10, yPos + 2, 300, 25), itemNameWithRarity, itemLabelStyle);

                    if (GUI.Button(new Rect(320, yPos + 2, 70, 24), "+1", buttonStyle))
                    {
                        GiveItem(filteredItems[i], 1);
                    }

                    if (GUI.Button(new Rect(400, yPos + 2, 70, 24), "+5", buttonStyle))
                    {
                        GiveItem(filteredItems[i], 5);
                    }

                    if (GUI.Button(new Rect(480, yPos + 2, 70, 24), "+10", buttonStyle))
                    {
                        GiveItem(filteredItems[i], 10);
                    }

                    if (GUI.Button(new Rect(560, yPos + 2, 70, 24), "+50", buttonStyle))
                    {
                        GiveItem(filteredItems[i], 50);
                    }

                    yPos += 30;
                }

                GUI.EndScrollView();

                yOffset += 420;

                // Footer
                GUI.Box(new Rect(10, yOffset, 730, 2), "");
                var footerStyle = new GUIStyle(labelStyle);
                footerStyle.fontSize = 11;
                footerStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
                GUI.Label(new Rect(10, yOffset + 5, 730, 25),
                    "💡 Type to search • Backspace to delete • Drag title bar • F3 to close",
                    footerStyle);
            }
            catch (Exception ex)
            {
                log.LogError($"DrawWindow error: {ex.Message}");
            }
        }

        private static string GetItemNameWithRarity(EItem item, int itemIndex)
        {
            try
            {
                var dataManager = DataManager.Instance;
                if (dataManager == null) return itemNames[itemIndex];

                var itemData = dataManager.GetItem(item);
                if (itemData == null) return itemNames[itemIndex];

                string rarityTag = "";
                switch (itemData.rarity)
                {
                    case EItemRarity.Legendary: rarityTag = " [L]"; break;
                    case EItemRarity.Epic: rarityTag = " [E]"; break;
                    case EItemRarity.Rare: rarityTag = " [R]"; break;
                    case EItemRarity.Common: rarityTag = " [C]"; break;
                    case EItemRarity.Corrupted: rarityTag = " [X]"; break;
                    case EItemRarity.Quest: rarityTag = " [Q]"; break;
                }

                return itemNames[itemIndex] + rarityTag;
            }
            catch
            {
                return itemNames[itemIndex];
            }
        }

        private static Color GetRarityColor(EItem item)
        {
            try
            {
                var dataManager = DataManager.Instance;
                if (dataManager == null) return Color.white;

                var itemData = dataManager.GetItem(item);
                if (itemData == null) return Color.white;

                switch (itemData.rarity)
                {
                    case EItemRarity.Legendary: return new Color(1.0f, 0.84f, 0.0f); // Gold
                    case EItemRarity.Epic: return new Color(0.64f, 0.21f, 0.93f);   // Purple
                    case EItemRarity.Rare: return new Color(0.25f, 0.55f, 1.0f);    // Blue
                    case EItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f);    // Light gray
                    case EItemRarity.Corrupted: return new Color(0.9f, 0.2f, 0.2f); // Red
                    case EItemRarity.Quest: return new Color(0.2f, 1.0f, 0.5f);     // Green
                    default: return Color.white;
                }
            }
            catch
            {
                return Color.white;
            }
        }

        private static void GiveAllItems()
        {
            try
            {
                var gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    log.LogWarning("GameManager not found!");
                    return;
                }

                var playerInventory = gameManager.GetPlayerInventory();
                if (playerInventory == null)
                {
                    log.LogWarning("Player inventory not found!");
                    return;
                }

                var dataManager = DataManager.Instance;
                if (dataManager == null)
                {
                    log.LogWarning("DataManager not found!");
                    return;
                }

                int count = 0;
                foreach (var item in allItems)
                {
                    try
                    {
                        var itemData = dataManager.GetItem(item);
                        if (itemData != null)
                        {
                            playerInventory.itemInventory.AddItem(item, 1);
                            count++;
                        }
                    }
                    catch { }
                }

                log.LogInfo($"Gave 1 of all {count} items!");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to give all items: {ex.Message}");
            }
        }

        private static void GiveItemsByRarity(EItemRarity targetRarity)
        {
            try
            {
                var gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    log.LogWarning("GameManager not found!");
                    return;
                }

                var playerInventory = gameManager.GetPlayerInventory();
                if (playerInventory == null)
                {
                    log.LogWarning("Player inventory not found!");
                    return;
                }

                var dataManager = DataManager.Instance;
                if (dataManager == null)
                {
                    log.LogWarning("DataManager not found!");
                    return;
                }

                int count = 0;
                foreach (var item in allItems)
                {
                    try
                    {
                        var itemData = dataManager.GetItem(item);
                        if (itemData != null && itemData.rarity == targetRarity)
                        {
                            playerInventory.itemInventory.AddItem(item, 1);
                            count++;
                        }
                    }
                    catch { }
                }

                log.LogInfo($"Gave {count} {targetRarity} items!");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to give {targetRarity} items: {ex.Message}");
            }
        }

        private static EItem[] GetFilteredItems()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return allItems;
            }

            return allItems.Where((item, index) =>
                itemNames[index].ToLower().Contains(searchText.ToLower())
            ).ToArray();
        }

        private static void GiveItem(EItem item, int count)
        {
            try
            {
                var gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    log.LogWarning("GameManager not found!");
                    return;
                }

                var playerInventory = gameManager.GetPlayerInventory();
                if (playerInventory == null)
                {
                    log.LogWarning("Player inventory not found!");
                    return;
                }

                playerInventory.itemInventory.AddItem(item, count);
                log.LogInfo($"Gave {count}x {item}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to give item {item}: {ex.Message}");
            }
        }
    }
}
