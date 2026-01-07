# Unified Grid & Button-Based Item Spawning - Setup Guide

## Summary of Changes

### New Files Created:
1. **GridManager.cs** - Manages unified grid, finds empty tiles, handles placement
2. **ItemButton.cs** - Attached to spawn buttons, handles item spawning on click

### Modified Files:
1. **ItemSpawner.cs** - Simplified to handle shuffle system only

---

## Setup Instructions

### Step 1: Add GridManager to Your Scene

1. **Create an empty GameObject** called `GridManager` at the root of your game scene
2. **Add the GridManager script** to this object
3. **In the Inspector**:
   - The GridManager will automatically find all `Tile` components in children on Start
   - OR manually assign all tiles to the `allTiles` array

### Step 2: Setup Spawn Buttons

You should have buttons in your hotbar/UI. For each button:

1. **Select the button GameObject** in your hierarchy
2. **Add the ItemButton script** component
3. **In the Inspector, configure**:
   - **Item Prefab**: Drag the item prefab you want to spawn (e.g., Chicken, Apple, Bowl, etc.)
   - **Button Image**: The Image component showing what item it spawns (optional - can auto-detect)
   - **Button**: Auto-detected, or manually assign the Button component

### Example Setup:
```
Canvas
├── HotBar (Horizontal Layout Group)
│   ├── SpawnFoodButton1
│   │   └── ItemButton.cs
│   │       - itemPrefab: FoodItem1 prefab
│   │       - buttonImage: Auto-set from prefab sprite
│   │
│   ├── SpawnFoodButton2
│   │   └── ItemButton.cs
│   │       - itemPrefab: FoodItem2 prefab
│   │
│   └── SpawnBowlButton
│       └── ItemButton.cs
│           - itemPrefab: Bowl prefab
```

### Step 3: Update ItemSpawner Reference

1. **Select your ItemSpawner GameObject** in the scene
2. **In the Inspector**:
   - Keep the `Shuffle Display` text field (shows how many shuffles left)
   - Assign the **Shuffle Button** reference
   - Keep the `shuffleCount` at 5 (or your desired value)

### Step 4: Verify Item References

Make sure your item prefabs have:
- ✅ `Item` component (for `ResetAnim()` call)
- ✅ `SpriteRenderer` component
- ✅ `Collider2D` for interactions
- ✅ Correct tags (e.g., "Food", "Bowl")

---

## How It Works

### Player Interaction Flow:

```
1. Player clicks Spawn Button
   ↓
2. ItemButton.OnButtonClicked() fires
   ↓
3. Asks GridManager for random empty Tile
   ↓
4. Instantiates item prefab at tile position
   ↓
5. Tile.TakeObject() claims the tile as full
   ↓
6. Item appears on grid, ready to be moved/merged
```

### Key Differences from Old System:

| Old System | New System |
|-----------|-----------|
| Items in hotbar slots | Items spawn directly on grid |
| Drag from slot to grid | Click button to spawn |
| Separate bowl/food areas | Unified grid (all items anywhere) |
| Manual hotbar management | Automatic tile placement |

---

## Game Mechanics That Still Work

✅ **Merging** - Items still merge via Grab.cs (drag two items together)
✅ **Bowl + Food Combos** - Tag checks still work (IFeedDog interface)
✅ **Combo Multiplier** - Hunger feedback works as before
✅ **Shuffle** - Randomizes positions of existing items
✅ **Grid Collision** - No overlapping items on same tile

---

## Common Issues & Fixes

### Problem: "No empty tiles available"
**Solution**: Grid is full. Either:
- Increase grid size
- Let player merge more items
- Clear some items from grid

### Problem: Items not spawning visually
**Solution**: Check that:
- Item prefab has a SpriteRenderer
- Tile position is within camera view
- Item prefab is a valid GameObject

### Problem: Button doesn't respond
**Solution**: Check that:
- ItemButton script is on button GameObject
- itemPrefab field is assigned (not null)
- GridManager exists in scene
- Button component has ItemButton.OnButtonClicked() listener

---

## Optional: Remove Old Hotbar References

If you want to completely remove old hotbar system:

1. Delete old `ItemSlot` GameObjects (if separate from buttons)
2. Remove `IItemSlot` interface usage (no longer needed)
3. Keep only the buttons with ItemButton.cs scripts

---

## Testing Checklist

- [ ] GridManager finds all tiles on Start
- [ ] Clicking button spawns item on random empty tile
- [ ] Multiple buttons spawn different items
- [ ] Items can be dragged around grid
- [ ] Two items merge when placed together
- [ ] Shuffle button randomizes item positions
- [ ] Bowl + Food combo triggers FeedDog() correctly
- [ ] No items overlap on same tile
- [ ] Hunger bar updates correctly

---

Done! Your game now has a modern merge-game style grid system. 🎮
