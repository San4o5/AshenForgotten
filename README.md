# 🔥 Ashen Forgotten

> A 2D action-platformer built with Unity 6 and C#, focused on precise, responsive movement and a clean, pattern-driven architecture.

> 🇺🇦 Українська версія: [README_UA.md](README_UA.md)

<!-- TODO: add a gameplay GIF or screenshot here, e.g.: -->
<!-- ![Gameplay](Docs/gameplay.gif) -->

---

## 📌 About

**Ashen Forgotten** is a metroidvania-style 2D platformer. The player explores hostile levels, fights enemies with distinct AI behaviors, collects coins and tries to survive.

The project is also an architecture playground: gameplay systems are built on classic design patterns (Strategy, Template Method, Service Locator, DI through interfaces) — see [ARCHITECTURE.md](ARCHITECTURE.md) for a detailed breakdown of every decision.

---

## ✨ Features

### 🎮 Movement that feels right
- Custom **kinematic character controller** — manual `Physics2D.CapsuleCast` instead of a Dynamic Rigidbody, eliminating "sticky walls" and unpredictable jumps (the approach used by Hollow Knight and Celeste)
- **Coyote time** (0.1 s) — you can still jump right after leaving a ledge
- **Jump buffering** (0.1 s) — a jump pressed just before landing is queued and fires on touchdown
- **Variable jump height** — releasing the jump button early cuts the jump exactly once

### ⚔️ Combat
- `HitInfo` struct carries full hit context (damage, direction, knockback force) instead of a bare number — consistent knockback for player and enemies
- **One swing = one hit**: `DamageDealer` tracks targets per attack via a `HashSet`
- Hitbox lifecycle driven by **Animation Events**
- **Hitstop** effect for impactful hits

### 🧠 Enemy AI
- Each enemy type is a pluggable "brain" implementing `IEnemyBrain`:
  - **Ash Servant** — patrols and chases the player (`PatrolChaseBrain`)
  - **Slime** — hops toward the target (`SlimeHopBrain`)
- Taking damage immediately forces the Chase state, so knockback is never swallowed
- Edge detection (`Physics2D.OverlapCircle`) reverses patrol at ledges

### 💰 Progression & UI
- Coin collection with a persistent wallet (`PlayerWallet`, backed by `PlayerPrefs`)
- Health HUD, coins HUD, death screen, main menu

---

## 🛠️ Tech Stack

| | |
|---|---|
| Engine | Unity 6 (6000.4.0f1), Universal Render Pipeline (URP) |
| Language | C# |
| Input | Unity Input System |
| Rendering extras | ShaderLab / HLSL shaders |

---

## 📂 Project Structure

```
Assets/_Project/Scripts/
├── Camera/           # CameraFollow
├── Combat/           # Health, DamageDealer, HitInfo, Hitstop, AttackHitbox
│   └── Interfaces/   # IDamageable, IAttacker, IHealth, IKnockbackReceiver
├── Enemy/            # EnemyController, EnemyHealth, controllers per enemy type
│   ├── Brains/       # PatrolChaseBrain, SlimeHopBrain (Strategy pattern)
│   └── Interfaces/   # IEnemyBrain, IBrainHitNotifier
├── Items/            # Coin + ICollectible
├── Player/           # PlayerMotor, PlayerCombat, PlayerHealth, PlayerFacade,
│   │                 # PlayerWallet, PlayerService
│   └── Interfaces/   # IPlayerInput, IPlayerMotor, IPlayerControl
└── UI/               # HealthHud, CoinsHud, DeathScreen, MainMenuController
```

---

## 🏗️ Architecture Highlights

| Pattern | Where | Why |
|---|---|---|
| **Strategy** | Enemy AI (`IEnemyBrain`) | Each enemy behavior is swappable without touching controllers |
| **Template Method** | Damage system (`Health` → `PlayerHealth` / `EnemyHealth`) | Shared skeleton (invulnerability, events, death), specific reactions in subclasses |
| **Service Locator** | `PlayerService` | O(1) player lookup for enemies, no per-frame `Find` calls |
| **DI via interfaces** | `PlayerMotor` ← `IPlayerInput` | Input can be replaced by AI or replay without code changes |
| **Facade** | `PlayerFacade` | Single point that syncs the Animator; other components only raise parameters |

Full write-up with reasoning behind every decision: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 🚀 Getting Started

1. Install **Unity 6000.4.0f1** (or a newer Unity 6 release) via Unity Hub
2. Clone the repository:
   ```bash
   git clone https://github.com/San4o5/AshenForgotten.git
   ```
3. Open the project folder in Unity Hub
4. Open the main scene from `Assets/Scenes/` and press **Play**

### 🎮 Controls

<!-- TODO: verify the bindings in InputSystem_Actions.inputactions -->
| Action | Key |
|---|---|
| Move | `A` / `D` |
| Jump | `Space` (hold for higher jump) |
| Attack | `Left Mouse Button` |

---

## 👤 Author

**Oleksandr Babaryka** — [github.com/San4o5](https://github.com/San4o5)
