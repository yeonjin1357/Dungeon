# Development Guidelines

## Project Architecture

### Mandatory Directory Structure
- **ALWAYS** place scripts in correct subfolder under Assets/Scripts/
  - Player/ - PlayerController, PlayerStats, PlayerInventory ONLY
  - Enemies/ - All enemy AI, behaviors, spawners
  - Items/ - Item data, NOT inventory (inventory goes in Player/)
  - Dungeon/ - Generation algorithms, room management, tilemap control
  - Combat/ - Combat calculations, turn management, damage systems
  - UI/ - ALL UI controllers and handlers
  - Managers/ - Singletons and system managers ONLY
- **NEVER** create scripts in Scripts/ root directory
- **NEVER** place utility functions outside Managers/ folder

### Namespace Requirements
- **MUST** use namespace Dungeon.{Category} matching folder name
- **FORBIDDEN** to use global namespace for any game scripts

## Code Standards

### Script Structure Template
```csharp
using UnityEngine;

namespace Dungeon.{Category}
{
    public class {ClassName} : MonoBehaviour
    {
        #region Fields
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        
        private int currentValue;
        private const int MAX_VALUE = 100;
        #endregion

        #region Unity Lifecycle
        private void Awake() { }
        private void Start() { }
        #endregion

        #region Public Methods
        public void PublicMethod() { }
        #endregion

        #region Private Methods
        private void PrivateMethod() { }
        #endregion
    }
}
```

### Naming Conventions
- **Classes**: PascalCase - `PlayerController`
- **Methods**: PascalCase - `TakeDamage()`
- **Private fields**: camelCase - `currentHealth`
- **Unity references**: _ prefix - `[SerializeField] private Rigidbody2D _rigidbody`
- **Constants**: UPPER_SNAKE_CASE - `MAX_INVENTORY_SIZE`
- **File names**: MUST match class name exactly

## Cross-File Dependencies

### Mandatory Update Chains
- **When modifying PlayerStats**:
  1. UPDATE UI/HUD/HealthBar.cs
  2. UPDATE UI/HUD/StatsDisplay.cs
  3. VERIFY Combat/DamageCalculator.cs compatibility

- **When adding new enemy type**:
  1. CREATE Enemies/{EnemyName}AI.cs
  2. REGISTER in Enemies/EnemyDatabase.cs
  3. ADD to Managers/PoolManager.cs object pools
  4. CREATE Prefabs/Enemies/{EnemyName}.prefab

- **When adding new item**:
  1. CREATE Items/Data/{ItemName}Data.cs as ScriptableObject
  2. REGISTER in Items/ItemDatabase.cs
  3. UPDATE UI/InventoryUI.cs if new item type
  4. ADD sprite to Assets/Sprites/Items/

- **When modifying combat formulas**:
  1. UPDATE Combat/CombatSystem.cs
  2. UPDATE Combat/DamageCalculator.cs
  3. UPDATE UI/DamagePopup.cs for display
  4. VERIFY Combat/TurnManager.cs timing

## Performance Requirements

### Pre-Feature Checklist
- **PROFILE** current metrics before implementation
- **VERIFY** draw calls remain <100
- **CHECK** SetPass calls remain <50
- **CONFIRM** memory usage <1GB

### Object Pooling Rules
- **Enemies**: Minimum pool size 20, expand by 10
- **Projectiles**: Minimum pool size 50, expand by 25
- **Effects**: Minimum pool size 30, expand by 15
- **UI Elements**: Pool all damage numbers and floating text

### Sprite Atlas Requirements
- **Maximum size**: 2048x2048 pixels
- **Group by**: Usage frequency and scene
- **Compression**: ETC2 (Android), PVRTC (iOS)
- **NEVER** use loose sprites in production builds

## Mobile Optimization

### Input System
- **USE ONLY** Unity Input System for touch
- **FORBIDDEN**: Input.GetTouch(), Input.mousePosition
- **REQUIRED** gestures:
  - Tap: Movement and interaction
  - Hold: Examine and context menu
  - Swipe: Camera pan
  - Pinch: Zoom control

### Screen Adaptation
- **TEST** on Device Simulator with:
  - 16:9 (standard phones)
  - 18:9 (modern phones)  
  - 19.5:9 (tall phones)
  - 4:3 (tablets)
- **USE** Safe Area for UI elements
- **ANCHOR** UI elements to screen edges

## Build Configuration

### Platform Settings
- **Android**:
  - Minimum API Level: 24 (Android 7.0)
  - Target API Level: 30+
  - Scripting Backend: IL2CPP
  - Architecture: ARMv7 + ARM64

- **iOS**:
  - Minimum version: iOS 12.0
  - Architecture: ARM64 only
  - Scripting Backend: IL2CPP

## Prohibited Actions

### Code Practices
- **NEVER** use Resources.Load() - use direct references or Addressables
- **NEVER** use GameObject.Find() in runtime code - cache references
- **NEVER** use FindObjectOfType() in Update loops
- **NEVER** concatenate strings in loops - use StringBuilder
- **NEVER** instantiate without pooling for recurring objects
- **NEVER** modify transform in FixedUpdate for non-physics objects

### Project Management
- **NEVER** auto-commit without explicit user request
- **NEVER** modify git configuration
- **NEVER** create README or documentation unless requested
- **NEVER** delete completed tasks from task manager
- **NEVER** use emojis in code or comments

## Decision Trees

### When Adding New Feature
1. **Performance impact?** → Profile first, implement second
2. **Similar system exists?** → Extend existing Manager, don't duplicate
3. **Affects combat?** → Update TurnManager + CombatSystem + UI
4. **UI element?** → Test all aspect ratios before finalizing
5. **New asset type?** → Create ScriptableObject template first

### When Fixing Bugs
1. **Performance related?** → Profile before and after fix
2. **Combat related?** → Test full combat cycle
3. **UI related?** → Verify on all screen sizes
4. **Save system related?** → Test save/load cycle completely

### When Optimizing
1. **Measure current state** → Use Unity Profiler
2. **Identify bottleneck** → Focus on biggest impact
3. **Implement solution** → Follow pooling/batching rules
4. **Verify improvement** → Compare before/after metrics
5. **Document changes** → Update relevant Manager comments

## Testing Requirements

### Per-Feature Testing
- **Runtime performance**: Maintain 60 FPS on target devices
- **Memory footprint**: Stay under 1GB total
- **Battery usage**: Test 15-minute gameplay sessions
- **Touch responsiveness**: <100ms response time
- **Scene transitions**: <2 second load times

### Pre-Build Checklist
- [ ] All prefabs updated and applied
- [ ] Sprite atlases regenerated
- [ ] Lightmaps baked (if applicable)
- [ ] Build settings configured for platform
- [ ] Development build disabled
- [ ] Profiler disabled in release

## Workflow Standards

### Feature Implementation Flow
1. Check existing systems for similar functionality
2. Create required scripts in proper directories
3. Implement with performance constraints
4. Update all dependent systems
5. Profile and optimize
6. Test on target aspect ratios

### Bug Fix Flow
1. Reproduce issue consistently
2. Profile if performance-related
3. Implement fix following conventions
4. Test all affected systems
5. Verify no regression

### Commit Message Format
- **Pattern**: `type: component - description`
- **Types**: feat, fix, refactor, perf, test
- **Examples**:
  - `feat: player - add dodge ability`
  - `fix: dungeon - correct room overlap`
  - `perf: enemies - implement object pooling`

## Unity-Specific Rules

### Prefab Management
- **ALWAYS** unpack prefab before major modifications
- **APPLY** changes to prefab after testing
- **USE** prefab variants for enemy/item variations
- **NEVER** break prefab connections without reason

### Scene Organization
- **Hierarchy structure**:
  ```
  -- Managers (DontDestroyOnLoad)
  -- Environment
     -- Tilemap
     -- Props
  -- Characters
     -- Player
     -- Enemies
  -- UI
     -- Canvas
     -- EventSystem
  -- Cameras
  -- Lighting
  ```

### Component Order
1. Transform (implicit)
2. Rendering components
3. Physics components
4. Custom scripts (logic)
5. Audio components

## Resource Management

### Asset Naming
- **Sprites**: `spr_category_name_variant`
- **Prefabs**: `pfb_category_name`
- **Materials**: `mat_name_variant`
- **Audio**: `sfx_action` or `bgm_scene`
- **Animations**: `anim_character_action`

### Memory Limits
- **Single texture**: Max 2048x2048
- **Audio clip**: Max 10MB uncompressed
- **Scene size**: Max 50MB
- **Total RAM**: Max 1GB at runtime

## Unity MCP Usage Guidelines

### Unity MCP Limitations
- **READ ONLY**: Unity MCP should only be used for reading/inspecting Unity state
- **NO MODIFICATIONS**: Do NOT use Unity MCP to modify GameObjects, components, or assets
- **MANUAL SETUP**: All Unity Editor setup must be done manually by the developer

### Unity Editor Setup Process
When Unity Editor configuration is needed:
1. **Assistant provides detailed step-by-step instructions**
2. **Developer manually performs the setup in Unity Editor**
3. **Assistant can verify the setup using Unity MCP read operations**

### Permitted Unity MCP Operations
- `manage_scene` with action: "get_hierarchy" - Check scene structure
- `manage_gameobject` with action: "get_components" - Inspect components
- `manage_asset` with action: "search" - Find existing assets
- `read_console` - Check for errors and warnings
- `manage_editor` with action: "get_state" - Verify editor state

### Forbidden Unity MCP Operations
- Creating or modifying GameObjects
- Adding or removing components
- Modifying component properties
- Creating or modifying assets
- Executing menu items
- Any write operations to the Unity project

### Workflow for Unity Tasks
1. **Code Creation**: Assistant creates all necessary C# scripts
2. **Instruction Delivery**: Assistant provides detailed Unity Editor setup instructions
3. **Manual Execution**: Developer performs setup in Unity Editor
4. **Verification**: Assistant uses Unity MCP read operations to verify if needed
5. **Troubleshooting**: If issues arise, assistant provides updated instructions

## Critical Paths

### Must Never Break
1. **Player spawn** in new dungeon
2. **Save/Load** cycle integrity
3. **Combat turn** order
4. **Inventory** item persistence
5. **Scene transitions** without data loss

### Always Verify
1. **Touch input** responsiveness
2. **UI scaling** on different devices
3. **Memory usage** after scene changes
4. **Object pool** recycling
5. **Sprite batch** count

---
*These rules are mandatory for all development on the Pixel Dungeon Clone project. Non-compliance will result in technical debt and performance issues.*