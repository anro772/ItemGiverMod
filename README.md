# ItemGiver Mod v2.0 for Megabonk

A BepInEx mod that adds a powerful item spawning menu to Megabonk.

## Features

### 🎁 Bulk Actions
- **Give All Items x1** - Instantly add 1 of every item in the game
- **All Legendary x1** - Add 1 of every Legendary rarity item
- **All Epic x1** - Add 1 of every Epic rarity item
- **All Rare x1** - Add 1 of every Rare rarity item
- **All Common x1** - Add 1 of every Common rarity item

### 🔍 Individual Item Control
- **Search**: Type to filter items by name
- **Per-item buttons**: +1, +5, +10, +50 for each item
- **Color-coded rarity**: Items are color-coded by rarity
  - Gold = Legendary [L]
  - Purple = Epic [E]
  - Blue = Rare [R]
  - Gray = Common [C]
  - Red = Corrupted [X]
  - Green = Quest [Q]

### 🎨 UI Features
- Clean, modern dark theme
- Searchable item list with 87+ items
- Draggable window
- Scrollable list
- Real-time keyboard input (no text field needed)

## Controls
- **F3** - Toggle the item menu on/off
- **Type** - Search/filter items while menu is open
- **Backspace** - Delete search text
- **Drag title bar** - Move the window

## Installation
1. Make sure BepInEx is installed in your Megabonk game folder
2. Copy `ItemGiver.dll` to `Megabonk\BepInEx\plugins\`
3. Launch the game
4. Start a run and press F3

## Requirements
- BepInEx 6.0+
- Megabonk game
- .NET 6.0 (included with BepInEx)

## Building from Source
```bash
cd project/ItemGiverMod
dotnet build --configuration Release
```

The compiled DLL will be at: `bin/Release/net6.0/ItemGiver.dll`

## Project Structure
```
project/ItemGiverMod/
├── Plugin.cs              # Main mod code
├── ItemGiverMod.csproj    # Project configuration
└── README.md              # This file
```

## Version History

### v2.0.1
- Fixed transparency bug when transitioning between runs
- Window now properly reinitializes styles when texture is destroyed

### v2.0.0
- Added bulk action buttons for all items
- Added rarity-based bulk actions (Legendary, Epic, Rare, Common)
- Added color-coded items by rarity
- Added rarity tags ([L], [E], [R], [C], [X], [Q])
- Improved UI layout with bulk actions section
- Reorganized project structure

### v1.0.x
- Initial release
- Basic item spawning
- Search functionality
- Per-item quantity buttons

## License
Free to use and modify

## Author
Created for Megabonk modding
