# Isle of the Primal Elements

> A top-down 2D action-adventure game built in Unity. Master four elemental powers, defeat ancient guardians, and confront the fallen lord Zerath in his fortress. Available as a WebGL build.

![Unity](https://img.shields.io/badge/Unity-2022.3-000000?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-11.0-239120?style=flat-square&logo=csharp)
![Platform](https://img.shields.io/badge/platform-WebGL%20%7C%20Standalone-9742FF?style=flat-square)
![Scenes](https://img.shields.io/badge/scenes-8-3B82F6?style=flat-square)
![Scripts](https://img.shields.io/badge/scripts-113-22C55E?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-9742FF?style=flat-square)

---

## 🎮 Play Now

A WebGL build is included in the repository. Open `docs/index.html` in a browser that supports WebGL 2.0, or serve the `docs/` directory behind any static HTTP server.

```bash
python3 -m http.server 8000
# → http://localhost:8000/docs/
```

Or play on a GitHub Pages deployment once enabled.

---

## Overview

*Isle of the Primal Elements* is a complete 2D action-adventure game built as a final project at Western University. You play as a chosen hero who must journey to four elemental domains, defeat the ancient guardians, collect their powers, and finally breach Zerath's Fortress to reclaim the Primal Element.

The game features a top-down pixel art aesthetic, twin-stick-style combat with a charged water projectile system, platforming-style hazard avoidance, boss fights with multi-phase mechanics, an NPC-driven hub world, and a persistent progression system.

---

## Story

The Primal Elements — Fire, Water, Earth, and Lightning — once maintained balance across the realm. When the corrupted lord Zerath seized the elements for himself, they fractured and empowered four elemental guardians:

- **Pyronis** — Lord of the Flames (Fire Domain)
- **Aqualis** — Keeper of the Tides (Water Domain)  
- **Terradon** — Warden of the Earth (Earth Domain)
- **Voltaris** — Herald of the Storm (Lightning Domain)

Defeat each guardian to absorb their element and gain new abilities. Only with all four can you breach **Zerath's Fortress** and end his reign.

---

## Game Structure

### Scenes

| Scene | Type | Description |
|-------|------|-------------|
| `CharacterSelect` | Menu | Choose your hero (Blue or Red) |
| `HubWorld` | Hub | Central hub with NPCs and biome portals |
| `MainIsland` | Hub | Alternate hub with biome access |
| `FireLevel` | Biome | Pyronis's volcanic domain |
| `WaterLevel` | Biome | Aqualis's flooded trenches |
| `EarthLevel` | Biome | Terradon's caverns |
| `LightningLevel` | Biome | Voltaris's storm spire |
| `BossLevel` | Boss | Multi-phase elemental gauntlet |
| `FinalBossScene` | Boss | Zerath's Fortress — final confrontation |
| `VictoryScene` | End | Post-credits resolution |
| `HouseScene` | Interlude | Safe zone / narrative beat |

### Element Cycle

Each element has a strength/weakness relationship:

```
Earth ──beats──▶ Lightning ──beats──▶ Water ──beats──▶ Fire ──beats──▶ Earth
```

This cycles through the guardian encounters and affects the final boss balance mechanics.

---

## Systems

### Player

- **MovementSystem** — 4-directional top-down movement with rigidbody velocity control
- **DashSystem** — Directional dash with cooldown, grants i-frames
- **PlayerCombat** — Melee attack with configurable range, cooldown, and hitbox. Detects the closest enemy in the facing direction using `OverlapCircleAll` with a fallback `TriggerCollider2D` hitbox
- **Water Wave Ability** — Charged projectile attack unlocked after defeating Aqualis. Charge time (1.5s) scales the projectile into three tiers: small (0–33%), medium (33–66%), and large (66–100%), each with increasing damage multiplier, speed, and visual scale
- **CharacterLoader** — Applies selected character (Blue/Red) from `PlayerPrefs` and swaps the animator override controller
- **PlayerHealth** — HP system with `IDamageable` interface support, death state, and respawn
- **PlayerStats** — Central stat aggregator; reads upgrade levels from `ShopManager` and pushes values to `PlayerCombat`, `MovementSystem`, etc.
- **PlayerGroundShockwave** — Area burst effect triggered on landing or special events
- **CollisionHandler** — Processes trigger overlaps for hazards, items, and zone boundaries
- **InteractionSystem** — Proximity-based interact trigger (NPCs, switches, portals)

### Abilities

The `AbilityManager` singleton powers a flexible ability system with four types:

| Type | Behavior | Examples |
|------|----------|---------|
| `Projectile` | Spawns a physics-driven object in the aim direction | Fireball, water wave |
| `AreaEffect` | Spawns a radius-based effect at player or target position | Ground slam, heal aura |
| `Buff` | Applies a temporary stat modifier (Speed, Defense, Attack, Health) | Speed boost, iron skin |
| `Melee` | Fires a directional overlap attack | Sword swing, dagger stab |

Abilities are data-driven through `AbilityData` ScriptableObjects, supporting configurable damage, cooldown, range, element type, and visual prefabs. The system includes runtime fallback projectile generation when no prefab is assigned.

### Boss Fights

Each biome features a unique boss encounter:

**Earth Boss (Terradon)**
- Multi-phase fight with 50% health phase transition
- Phase 1: Ground slam waves, chase AI, rock rain from ceiling
- Phase 2: Triple slam attack (3 waves at 30° spread), increased aggression
- Arena reveal trigger locks player in, intro cutscene with camera animation
- Drops Earth element on defeat, spawns exit portal

**BossController (Final Boss Arena)**
- Single arena cycles through all four elemental boss prefabs in sequence
- Transitions between forms preserve damage from the previous phase
- Each form inherits the `isFinalBoss` flag for special defeat handling
- Health bar UI tracks the current form

### Enemies

- **EnemyAI** — Patrol → Chase → Attack state machine with configurable ranges and speeds
- **EnemyAI2** — Alternate AI pattern used in Fire level
- **EnemyHealth** — Damageable via `IDamageable.TakeDamage()`, exposes `OnDied` and `OnHealthChanged` events, with configurable max HP and death reward (coin drops)
- **EnemyDamage** — Applies damage to player on contact with cooldown
- **EnemyHealthBar** — World-space HP bar above enemy
- **CoinPickup** — Physics-based collectible dropped on enemy death

### Shop & Upgrades

Coins collected from enemies and pickups are spent in the Hub World shop. The upgrade system is data-driven through ScriptableObjects:

| Upgrade | Effect | Max Level |
|---------|--------|-----------|
| **Health Potion** | Heals 50 HP on use (Q key) | Unbounded (per-potion purchase) |
| **Attack Damage** | Increases `PlayerCombat.attackDamage` | Configurable |
| **Attack Speed** | Reduces `PlayerCombat.attackCooldown` | Configurable |
| **Attack Range** | Increases `PlayerCombat.attackRange` | Configurable |
| **Move Speed** | Increases `MovementSystem.moveSpeed` | Configurable |
| **Max Health** | Increases `PlayerHealth.maxHealth` | Configurable |
| **Defense** | Reduces incoming damage | Configurable |

The `ItemEffectApplier` provides a clean pipeline for translating ShopUpgradeSO values into runtime stat changes through `PlayerStats`.

### Puzzles

- **Torch Puzzle (Fire)** — Light torches in sequence or proximity-based detection
- **Lever Puzzle (Water)** — Multi-lever drain challenges in three stages, controlling water levels
- **One-Way Door (Earth)** — Directional traversal restriction
- **Arena Reveal (Earth)** — Boss arena wall activates on trigger

### Environment Hazards

| Hazard | Effect | Level |
|--------|--------|-------|
| Lava Pool | Contact damage, visual + audio feedback | Fire |
| Lightning Beam | Periodic high-voltage damage | Lightning |
| Lightning Flicker | Ambient strobe effect on lights | Lightning |
| Water Drain | Hazard zone in submerged areas | Water |
| Falling Rocks | Ceiling collapse, spawner-driven | Earth |
| Collapse Zones | Destructible terrain on trigger | Earth |

### Save System

Persistent progression through `GameProgressManager` (singleton, `DontDestroyOnLoad`):

- Collects and tracks up to 4 elemental powers
- Marks level completion, boss defeats, coin totals
- Gate progression — Zerath's Fortress requires all 4 elements
- Element loss mechanic: failing to Zerath strips one element (must reclaim)
- `PlayerPrefs` serialization under the `Elementara_` key prefix
- Scene persistence via `SpawnManager.CaptureCurrentPlayerHealthForNextScene()`

### UI

| Component | Description |
|-----------|-------------|
| `HealthUI` | Hearts or bar displaying current/max HP |
| `AbilityUI` | Cooldown indicators for equipped abilities |
| `CoinUI` | Total coin counter from `GameProgressManager` |
| `BossHealthUI` | Screen-space boss HP bar with name label |
| `BiomeSelectionUI` | Hub world biome portal interaction panel |
| `CharacterSelectManager` | Character picker with PlayerPrefs persistence |
| `InteractionPromptUI` | Context-sensitive "Press E to interact" tooltip |
| `PlayerDeathScreenUI` | Death overlay with respawn/retry options |
| `CameraShake` | Screenshake effect on impacts and boss attacks |

---

## Project Structure

```
Assets/
├── Art/                        # Sprites, tilesets, character art, UI, portals
│   ├── Boss/                   # Boss-specific sprites and animations
│   ├── Characters/             # Player character sprite sheets
│   ├── Items/                  # Item and pickup sprites
│   ├── NPC's/                  # NPC portraits and sprites
│   ├── Portals/                # Biome portal and transition effects
│   ├── Sprites/                # Shared sprite assets
│   ├── Tilesets/               # Environment tilemaps
│   └── UI/                     # UI element sprites and panels
├── Audio/                      # Sound effects and music tracks
├── Data/                       # ScriptableObject definitions
│   ├── Levels/                 # LevelData assets (scene references, unlock conditions)
│   └── ShopItems/              # Shop catalog and upgrade definitions
├── Prefabs/                    # Reusable GameObject templates
│   ├── Bosses/                 # Boss prefabs (all forms)
│   ├── Enemies/                # Enemy prefabs (per biome)
│   ├── Hazards/                # Lava, lightning, falling rock prefabs
│   ├── Items/                  # Pickup and collectible prefabs
│   ├── Player/                 # Player prefab, components
│   └── UI/                     # Canvas and UI element prefabs
├── Scenes/                     # Game scenes
│   └── Levels/                 # Biome-specific scene files
├── Scripts/
│   ├── Abilities/              # Ability system (projectile, area, buff, melee)
│   ├── Bosses/                 # Boss management and arena logic
│   ├── Core/                   # Progression, interfaces, spawning, transitions
│   ├── EarthLevel/             # Earth biome-specific mechanics
│   ├── Enemies/                # Enemy AI, health, damage, drops
│   ├── Environment/            # Portals, hazards, puzzles, triggers
│   ├── Fire/                   # Fire biome-specific mechanics
│   ├── Hazards/                # Damage zones and environmental threats
│   ├── Items/                  # Item pickup logic
│   ├── NPC/                    # Dialogue and NPC interaction
│   ├── Player/                 # Movement, combat, health, stats, abilities
│   ├── Shop/                   # Shop logic, data, UI (full ECS-lite)
│   ├── UI/                     # All UI components and managers
│   └── WaterLevel/             # Water biome-specific mechanics and puzzles
├── Settings/                   # Build profiles and scene settings
├── TextMesh Pro/               # TMP fonts and resources
└── VFX/                        # Visual effects (Aqualis, coin, lightning)
```

---

## Controls

| Action | Input |
|--------|-------|
| Move | WASD / Arrow Keys |
| Attack (Melee) | Left Click / J |
| Water Wave | Q (hold to charge, release to fire) |
| Ability 1 | Left Click / J |
| Ability 2 | Right Click / K |
| Dash | Space / Shift |
| Interact | E |
| Use Health Potion | Q (when no wave unlocked) / dedicated key |

---

## Technical Details

### Key Scripts (~5,700 total lines of C#)

| Script | Lines | Purpose |
|--------|-------|---------|
| `GameProgressManager.cs` | 622 | Singleton save system, element/level/boss tracking |
| `AbilityManager.cs` | 590 | Modular ability system, 4 ability types with cooldowns |
| `PlayerCombat.cs` | 437 | Melee + charged water wave combat system |
| `EarthBossAI.cs` | 345 | Multi-phase boss AI with ground slam and rock rain |
| `PlayerController.cs` | 138 | Input routing to movement, collision, interaction |
| `ShopManager.cs` | 133 | Upgrade purchase logic, wallet, stat application |

### Patterns Used

- **Singleton** — `GameProgressManager`, `AbilityManager`, `SpawnManager`
- **Observer** — `EnemyHealth.OnDied`/`OnHealthChanged` events, `ShopManager.OnUpgradePurchased`
- **Strategy** — Ability type dispatch (`Projectile`/`AreaEffect`/`Buff`/`Melee`)
- **State Machine** — `EarthBossAI.BossState`, `EnemyAI` patrol/chase/attack
- **Interface** — `IDamageable`, `IInteractable` for polymorphic interaction
- **ScriptableObject** — `AbilityData`, `LevelData`, `ShopCatalogSO` for data-driven design

### Rendering

- 2D Orthographic camera with pixel-perfect unit scaling
- Sprite-based rendering with sorting layers (Ground, Decor, Enemies, Player, UI)
- URP 2D Renderer for lighting effects (torch flicker, lightning strobe)
- Sorting order management for depth-based y-axis rendering

---

## Building

### Requirements

- Unity 2022.3 LTS or later
- URP 2D Render Pipeline package

### Build Steps

1. Open project in Unity Hub
2. Open `Scenes/CharacterSelect` as the first scene in Build Settings
3. Add all scenes from `Assets/Settings/Scenes/` to build list
4. Build for target platform:
   - **WebGL**: `File → Build Settings → WebGL → Build`
   - **Standalone**: `File → Build Settings → Windows/macOS/Linux → Build`

### WebGL Build

The `docs/` directory contains a pre-built WebGL deployment. To update:

```bash
# Build to Assets/Settings/Build Profiles/, then copy output to docs/
```

---

## Credits

Built as the SE2250 final project — Western University Software Engineering.

- **Development Team**: Deepansh, Owen, Dylan, Sserkan, Mihir
- **Art**: Pixel art assets from open sources including *Enemy Galore 1* pack
- **Engine**: Unity 2022.3 LTS

---

## License

MIT — see LICENSE file for details.