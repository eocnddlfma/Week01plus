# Special Orbital Weapons Setup Guide

## Completed Files

### 1. Orbital Weapon Scripts (SSH folder)
- **BatOrbitalWeapon.cs** - Cleaned up unused `_myPivotPos` field
- **WallOrbitalWeapon.cs** - Enemy attachment and pulling mechanics
- **BombOrbitalWeapon.cs** - Explosion on impact with radius damage
- **PenetrationOrbitalWeapon.cs** - Speed acceleration on hit
- **BounceOrbitalWeapon.cs** - Wall bounce physics with collision bypass

### 2. Generator Scripts (SSH folder)
- **OrbitalWeaponPrefabGenerator.cs** - Creates prefabs for all 5 weapons
  - Menu: `Tools > Generate Special Orbital Weapon Prefabs`
  - Creates prefabs in `Assets/03. Prefab/Satellites/`

- **BallDataGenerator.cs** - Creates BallData SO assets
  - Menu: `Tools > Generate BallData Assets`
  - Requires prefabs to exist first
  - Creates assets in `Assets/00. SO/Balls/`

### 3. Data Scripts (Data folder)
- **BallData.cs** - ScriptableObject for ball selection UI
  - Contains: name, description, prefab reference, rarity, icon

## Setup Steps

### Step 1: Compile Scripts
1. Switch to Unity Editor
2. Wait for script compilation to complete
3. Check Console for any errors

### Step 2: Generate Prefabs
1. Go to `Tools > Generate Special Orbital Weapon Prefabs`
2. Wait for completion (check Console)
3. Verify 5 new prefabs created:
   - `BatOrbitalWeapon.prefab`
   - `WallOrbitalWeapon.prefab`
   - `BombOrbitalWeapon.prefab`
   - `PenetrationOrbitalWeapon.prefab`
   - `BounceOrbitalWeapon.prefab`

### Step 3: Generate BallData Assets
1. Go to `Tools > Generate BallData Assets`
2. Wait for completion (check Console)
3. Verify 5 new SO files created in `Assets/00. SO/Balls/`

### Step 4: Testing
- The weapons should now appear in ball selection UI during gameplay
- Test each weapon's special ability:
  - **Bat**: Swings at own location when player attacks
  - **Wall**: Attaches to and drags enemies
  - **Bomb**: Explodes with area damage on enemy hit
  - **Penetration**: Accelerates and changes color on hit
  - **Bounce**: Reflects off walls with speed reduction

## Files Modified From Previous Work
- **BatOrbitalWeapon.cs** - Removed unused `_myPivotPos` field (line 12)
- **UpgradeManager.cs** - Already contains BatAttackSpeed/Cooldown handling
- **PlayerStatModifier.cs** - Already contains bat upgrade multipliers

## Notes
- All weapons inherit from `OrbitalWeapon` base class
- Special abilities activate only during `BallState.Launched` state
- BatOrbitalWeapon uses rotation sync only (no position follow)
