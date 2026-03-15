# Week01plus — Unity Project Context for AI Agents

## Project Overview
A top-down 2D Unity action game where the player uses a bat weapon and orbital ball weapons to fight waves of enemies and bosses. Built with Unity (2D, orthographic camera), DOTween for animations, and the new Unity InputSystem.

**Branch:** 려차
**Engine:** Unity 2D, Orthographic Camera
**Key Libraries:** DOTween, Unity InputSystem, TextMeshPro

---

## Architecture Patterns

### 1. Global Event Bus — `GameEvents`
All cross-system communication goes through static events in `GameEvents.cs`. Never use direct references between unrelated systems.

```csharp
// Raising
GameEvents.RaisePlayerDamaged(currentHp);
GameEvents.RaiseWaveCleared(isBossWave);

// Subscribing (always unsubscribe in OnDisable/OnDestroy)
GameEvents.OnPlayerDamaged += HandleDamaged;
GameEvents.OnPlayerDamaged -= HandleDamaged;
```

Events: `OnGameStateChanged`, `OnScoreChanged`, `OnEnemyKilled`, `OnPlayerDamaged`, `OnWaveStarted`, `OnWaveCleared`, `OnUpgradeApplied`

### 2. ScriptableObject Data
All tunable data lives in SOs, never hardcoded. Always call `InitFromStatsData()` in Awake/Start.

| SO | Used By |
|----|---------|
| `PlayerStatsData` | `PlayerController` |
| `EnemyStatsData` | `EnemyBase` (all enemies) |
| `WeaponStatsData` | `WeaponChargeSystem` |
| `OrbitalStatsData` | `OrbitalWeapon` |
| `WaveData` | `WaveManager` |
| `UpgradeData` | `UpgradeManager` |
| `fbdfbd_SOBossSkillBase` (subclasses) | `BossBase` |

### 3. Object Pooling
Never `Instantiate`/`Destroy` enemies or projectiles at runtime.

| Pool | Manages |
|------|---------|
| `EnemyPoolManager` | All enemy prefabs (key: prefab instance ID) |
| `EnemyProjectilePool` | `EnemyProjectile` |
| `EnemyBossMineProjectilePool` | `EnemyBossMineProjectile` |
| `DamageTextPool` | Damage popup text |

Usage: `EnemyPoolManager.Instance.Get(prefab)` / return via `ResetState()` + `gameObject.SetActive(false)`.

### 4. PlayerStatModifier — Runtime Stat Multipliers
Applied on top of base SO values. Always multiply by this when computing final stats.

```csharp
float moveMult = PlayerStatModifier.Instance?.MoveSpeedMult ?? 1f;
```

Fields: `MoveSpeedMult`, `BatDamageMult`, `BallDamageMult`, `KnockbackMult`, `ChargeSpeedMult`, `BallSpeedMult`, `AttackRangeMult`, `DashCooldownMult`, `InvincibilityMult`

### 5. Singleton Pattern
Used by: `GameManager`, `UpgradeManager`, `EnemyPoolManager`, `EnemyProjectilePool`, `EnemyBossMineProjectilePool`, `DamageTextManager`, `PlayerStatModifier`

---

## Folder Structure — `Assets/02. Scripts/`

### `/Core` — Infrastructure
- **`GameEvents.cs`** — Static event bus (see above)
- **`ObjectPool<T>`** — Generic reusable pool
- **`EnemyPoolManager`** — Enemy GO pool by prefab
- **`EnemyProjectilePool`** — Regular projectile pool
- **`EnemyBossMineProjectilePool`** — Boss mine pool

### `/Data` — ScriptableObjects
- **`PlayerStatsData`** — maxHp, moveSpeed, dashSpeed, dashDuration, dashCooldown, invincibleDuration, flashInterval
- **`EnemyStatsData`** — hp, moveSpeed, stopDistance, scoreReward
- **`WeaponStatsData`** — chargeCooldown, maxChargeTime, 4× `ChargeLevelData` (rotationAngle, rotationDuration, damageAmount, knockbackForce, attackPower, hitStopDurationMult)
- **`OrbitalStatsData`** — orbitRadius, orbitAngularSpeed, launchSpeed, baseDamage, maxChargeDamage, knockbackForce, return physics
- **`WaveData`** — enemyList, spawn intervals, isBossWave flag

### `/Jaein` — Player & Weapon Systems

#### Player
- **`EntityBase`** (abstract) — Base for all combat entities. Has `TakeDamage()`, `Hp`, `MaxHp`, `IsDead`, `Rb`.
- **`PlayerBase`** (→ EntityBase) — HP, invincibility frames, flash effect, `IncreaseMaxHp()`.
- **`PlayerController`** (→ PlayerBase) — Top-level coordinator. Wires input → movement → dash → weapon.
- **`PlayerInputHandler`** — New InputSystem wrapper. Provides `InputVec`, `OnAttackInput`.
- **`PlayerMovement`** — Move, rotate toward mouse, boundary clamping.
- **`PlayerDash`** — Dash with cooldown, boundary-aware, `IsDashing`, `OnDashStateChanged`.

#### Weapon — Bat
The bat system is split into 3 components all on the same GameObject:

```
BatWeaponManager (coordinator)
├── WeaponChargeSystem  — charge timing, 4 levels, visual progression
├── WeaponSwingSystem   — swing rotation animation, hit detection
└── WeaponHitProcessor  — collision resolution (enemy damage, ball launch, projectile reflect)
```

**Charge levels (0–3):** Determined by `chargePercent` floored × 4. Stored in `WeaponStatsData.chargeLevels[]`.

**Knockback formula (EaseOutCubic):**
```csharp
float t = Mathf.Clamp01(force / maxForce);
float duration = (1f - (1f - t) * (1f - t) * (1f - t)) * maxDuration;
```

**Projectile handling by charge level:**
- Level 0: ignore
- Level 1: push (ReflectAsBatHit, 0 damage)
- Level 2: destroy
- Level 3: reflect with damage

**`WeaponChargeInfo`** — lightweight component storing `ChargePercent` on the weapon, read by `OrbitalWeapon` on collision.

#### Weapon — Orbital Ball (`OrbitalWeapon`)
Three states: `Orbit` → `Launched` → `Returning`

Key behaviors:
- Launched by bat swing (`TriggerLaunchFromWeapon(chargePercent, attackPower)`)
- `attackPower` = discrete value from `_chargeLevels[chargeLevel].attackPower` (not interpolated)
- Damage formula: `Lerp(baseDamage, maxChargeDamage, chargePercent) * attackPower * BallDamageMult`
- chargePercent resets to 0 after each enemy hit
- Knockback direction logic: if ball moving toward player (`dot > 0`) → deflect sideways; otherwise → travel direction
- Force = `velocity.magnitude * knockbackForce * KnockbackMult`
- Returns via gravity-like `returnStrength` pulling toward player center

Subclasses: `GravityOrbitalWeapon`, `SplitOrbitalWeapon` (in `/SSH`)

`OrbitalTrajectory` — LineRenderer trajectory prediction (reads `OrbitalWeapon.SimulateTrajectory()`).

### `/Ryeol` — Game Management & UI

- **`GameManager`** (singleton) — State machine: `Idle → Playing → GameOver/GameClear`. Tracks score and enemy count.
  ```csharp
  enum GameState { Idle, Playing, GameOver, GameClear }
  ```
- **`GameScene`** — Scene bootstrap. Listens to `GameEvents.OnGameStateChanged`, starts `WaveManager`.
- **`WaveManager`** — Spawns enemies from `WaveData[]`, manages wave progression, calls `GameEvents.RaiseWaveCleared()`.
- **`WaveData`** (SO) — List of `EnemySpawnInfo` (prefab + count), timing, `isBossWave`.
- **`CheatManager`** — Debug keys 1–6 for testing.

UI:
- **`UI_GameEndScreen`** — Game over/clear screen with DOTween animations.
- **`UI_HealthBar`** — Life icons that flash/fade on damage.
- **`UI_ButtonEffect`** — Hover/click animation on buttons.

### `/fbdfbd` — Enemy AI & Boss

#### `EnemyBase` (abstract, → EntityBase)
Core AI for all enemies.

Key systems:
- **Flocking:** `CalculateSeparation()` + `CalculateNoise()` for organic movement
- **Knockback:** `AddExternalVelocity(Vector2 vel, float duration)`. While `IsKnockedBack` is true, normal movement is frozen and velocity lerps to zero.
- **Attack scheduling:** `CanAttack()` / `DoAttack()` abstract, timer-based
- **Pooling:** `ResetState()` resets to SO values, `ShouldTrackEnemyCount` (false for clones)
- **Death:** fires `GameEvents.RaiseEnemyKilled()`, uses `scoreReward` from SO

Override points: `CanAttack()`, `DoAttack()`, `CalculateMoveDirection()`, `OnDeath()`, `ResetState()`

#### Enemy Types
| Class | Behavior |
|-------|----------|
| `EnemyMelee` | Contact damage only |
| `EnemyRange` | Fires `EnemyProjectile` from pool |
| `EnemyCharger` | Approach → Windup → Charge → Recover state machine |
| `EnemySplit` | Periodically spawns `EnemySplitClone` |
| `EnemySplitClone` | Clone; `ShouldTrackEnemyCount = false` |
| `EnemyMine` | Fires `EnemyBossMineProjectile` |

**`EnemyCharger` Note:** Overrides `FixedUpdate()`. Checks `IsKnockedBack` at the top and delegates to `base.FixedUpdate()` when knocked back — otherwise its own movement code would overwrite knockback.

#### `EnemyProjectile`
Implements `IEnemyProjectile`. Moves by setting `rb.position` (not linearVelocity). Has `Speed` property. `ReflectAsBatHit(damage, layerMask)` for bat reflection.

#### Boss System (`BossBase` → `EnemyBase`)
- Skills defined as `BossSkillSlot[]` (SO config + MonoBehaviour logic component)
- `PickReadySkillIndex()` → weighted random from ready skills
- Skill lifecycle: `Enter()` → `Execute()` (coroutine) → `Exit()`
- SO types: `fbdfbd_SOBossSkillShot`, `fbdfbd_SOBossSkillDash`, `fbdfbd_SOBossSkillMineShot`

### `/SSH` — Boss-related extras
- `GravityOrbitalWeapon`, `SplitOrbitalWeapon` — OrbitalWeapon subclasses
- `BossEnemyProjectile` — Has `Speed` property (implements `IEnemyProjectile`)

### `/Upgrade` — Upgrade System
Wave-clear roguelite upgrades.

- **`UpgradeData`** (SO) — `upgradeName`, `description`, `icon`, `rarity`, `statType` (enum), `value`, `prerequisites` (List<UpgradeData>)
- **`UpgradeManager`** (singleton) — On wave clear: pause time → show 3 random choices → apply selection → resume
  - Prerequisite check: all items in `prerequisites` must be in `_appliedUpgrades`
  - Stat types: `MoveSpeed`, `BatDamage`, `BallDamage`, `Knockback`, `ChargeSpeed`, `BallSpeed`, `AttackRange`, `DashCooldown`, `Invincibility`, `MaxHp`
- **`UpgradeUI`** — Animated card panel, fires callback on selection
- **`UpgradeCard`** — Individual card with `Setup(UpgradeData)`

```csharp
enum UpgradeStatType {
    MoveSpeed, BatDamage, BallDamage, Knockback, ChargeSpeed,
    BallSpeed, AttackRange, DashCooldown, Invincibility, MaxHp
}
```

### `/WS` — Visual Effects & Camera

- **`ChargeCameraEffect`** — Main camera controller
  - Weighted follow: `(playerPos * 4 + orbitalWeaponAvg * 6) / 10`
  - On charge: camera shifts toward player facing direction + zoom in
  - On full-charge release: impact shake + zoom punch
  - On damage: shake + kickback direction
  - DOTween methods: `TweenToSize()`, `TweenToPositionOffset()`
  - Public: `BeginCharge()`, `EndCharge(chargePercent)`, `sizeOffset`, `positionOffset`

- **`HitStopController`** — `Time.timeScale` manipulation for hit-stop. `TryPlayWithChargeAndHitCount()` scales duration by charge and hit count.
- **`DamageTextManager`** / **`DamageTextPool`** — World-to-canvas damage popups.
- **`EffectParticle`** — `Play(chargePercent)` wrapper around ParticleSystem.
- Various visual: `CircleLineRenderer`, `BoundaryRingAnimator`, `RingAnimation`, `HPEffect`, `WaveComingText`, `ScoreTextAnimation`

---

## Key Interfaces

```csharp
// Enemy/IEnemyProjectile.cs
interface IEnemyProjectile {
    Rigidbody2D Rb { get; }
    float Speed { get; }
    void ReflectAsBatHit(int damage, LayerMask layerMask);
}
```

---

## Game Flow

```
GameScene.Init()
  └─ GameManager.StartGame()
       └─ GameEvents.RaiseGameStateChanged(Playing)
            └─ WaveManager starts spawning from WaveData[]
                 ├─ EnemyPoolManager.Get(prefab) → enemy.ResetState() + SetTarget(player)
                 └─ when all enemies dead → GameEvents.RaiseWaveCleared(isBossWave)
                      ├─ UpgradeManager → pause, show cards, apply, resume
                      └─ WaveManager → next wave or GameManager.GameClear()
```

---

## Common Gotchas

1. **EnemyCharger ignores knockback by default** — it overrides `FixedUpdate()` and must check `IsKnockedBack` at the top to delegate to `base.FixedUpdate()`.

2. **EnemyProjectile uses `rb.position` not `rb.linearVelocity`** — do not read `rb.linearVelocity` to get its speed; use the `Speed` property from `IEnemyProjectile`.

3. **OrbitalWeapon chargePercent resets after each hit** — `_chargePercent = 0f; _attackPower = 1f` is called inside `OnTriggerEnter2D` after processing damage.

4. **attackPower is discrete by charge level** — use `_chargeLevels[Mathf.Min(3, Mathf.FloorToInt(chargePercent * 4f))].attackPower`, not `Lerp`.

5. **Camera XY follows weighted average** — `_basePosition.z` is still used for the Z axis; only XY is dynamic.

6. **Upgrades use prerequisites list** — an `UpgradeData` only appears in the selection pool if all items in its `prerequisites` list are already in `_appliedUpgrades`.

7. **PlayerStatModifier multipliers are additive to 1.0 base** — `value = 0.15` means `+15%`, so final = `base * (1 + sum_of_values)`. Exception: `MaxHp` is an integer addend, not a multiplier.

---

## File Map (abbreviated)

```
Assets/02. Scripts/
├── Core/
│   ├── GameEvents.cs
│   ├── ObjectPool.cs
│   ├── EnemyPoolManager.cs
│   ├── EnemyProjectilePool.cs
│   └── EnemyBossMineProjectilePool.cs
├── Data/
│   ├── PlayerStatsData.cs
│   ├── EnemyStatsData.cs
│   ├── WeaponStatsData.cs
│   ├── OrbitalStatsData.cs
│   └── WaveData.cs (in Ryeol)
├── Jaein/
│   ├── EntityBase.cs
│   ├── PlayerBase.cs
│   ├── PlayerController.cs
│   ├── PlayerInputHandler.cs
│   ├── PlayerMovement.cs
│   ├── PlayerDash.cs
│   ├── BatWeaponManager.cs
│   ├── WeaponChargeSystem.cs
│   ├── WeaponChargeInfo.cs
│   ├── WeaponSwingSystem.cs
│   ├── WeaponHitProcessor.cs
│   ├── OrbitalWeapon.cs
│   └── OrbitalTrajectory.cs
├── Ryeol/
│   ├── GameManager.cs
│   ├── GameScene.cs
│   ├── WaveManager.cs
│   ├── WaveData.cs
│   ├── CheatManager.cs
│   ├── UI_GameEndScreen.cs
│   ├── UI_HealthBar.cs
│   └── UI_ButtonEffect.cs
├── Enemy/
│   └── IEnemyProjectile.cs
├── fbdfbd/
│   ├── EnemyBase.cs
│   ├── EnemyProjectile.cs
│   ├── ContactDamageDealer.cs
│   ├── EnemyHitFlash.cs
│   ├── EnemyMelee.cs
│   ├── EnemyRange.cs
│   ├── EnemyCharger.cs
│   ├── EnemySplit.cs
│   ├── EnemySplitClone.cs
│   ├── EnemyMine.cs
│   ├── EnemyBossMineProjectile.cs
│   ├── BossBase.cs
│   ├── BossSkillBase.cs
│   └── BossSkills/
│       ├── fbdfbd_BossSkillShot.cs
│       ├── fbdfbd_BossSkillDash.cs
│       └── fbdfbd_BossSkillMineShot.cs
│   └── SimpleBossSkill/
│       ├── fbdfbd_SOBossSkillBase.cs
│       └── fbdfbd_SOBossSkillShot.cs (+ Dash, MineShot variants)
├── SSH/
│   ├── GravityOrbitalWeapon.cs
│   ├── SplitOrbitalWeapon.cs
│   └── Boss/BossEnemyProjectile.cs
├── Upgrade/
│   ├── UpgradeData.cs
│   ├── UpgradeManager.cs
│   ├── UpgradeUI.cs
│   └── UpgradeCard.cs
└── WS/
    ├── ChargeCameraEffect.cs
    ├── HitStopController.cs
    ├── DamageTextManager.cs
    ├── DamageTextPool.cs
    ├── EffectParticle.cs
    └── (visual effects...)
```
