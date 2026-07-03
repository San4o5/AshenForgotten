# 🔥 Ashen Forgotten (Попіл Забутих)

> 2D action-платформер на Unity 6 та C# з фокусом на точному, чуйному керуванні та чистій архітектурі на патернах проєктування.

> 🇬🇧 English version: [README.md](README.md)

<!-- TODO: додай гіфку або скриншот геймплею, наприклад: -->
<!-- ![Gameplay](Docs/gameplay.gif) -->

---

## 📌 Про гру

**Ashen Forgotten** — 2D-платформер у стилі metroidvania. Гравець досліджує ворожі рівні, б'ється з ворогами з різними моделями поведінки, збирає монети та намагається вижити.

Проєкт водночас є полігоном для архітектури: ігрові системи побудовані на класичних патернах (Strategy, Template Method, Service Locator, DI через інтерфейси) — детальний розбір кожного рішення у [ARCHITECTURE.md](ARCHITECTURE.md).

---

## ✨ Можливості

### 🎮 Керування, що відчувається правильно
- Кастомний **kinematic-контролер персонажа** — ручний `Physics2D.CapsuleCast` замість Dynamic Rigidbody, що усуває «липкі стіни» та непередбачувані стрибки (підхід Hollow Knight і Celeste)
- **Coyote time** (0.1 с) — стрибок можливий одразу після сходу з платформи
- **Буферизація стрибка** (0.1 с) — натиснутий перед приземленням стрибок спрацьовує при торканні землі
- **Змінна висота стрибка** — раннє відпускання кнопки обрізає стрибок рівно один раз

### ⚔️ Бойова система
- Структура `HitInfo` несе повний контекст удару (шкода, напрямок, сила відкидання) замість голого числа — консистентний knockback для гравця та ворогів
- **Один замах = один удар**: `DamageDealer` відстежує цілі атаки через `HashSet`
- Життєвий цикл хітбоксів керується через **Animation Events**
- Ефект **hitstop** для соковитих влучань

### 🧠 AI ворогів
- Кожен тип ворога — окремий «мозок», що реалізує `IEnemyBrain`:
  - **Ash Servant** — патрулює та переслідує гравця (`PatrolChaseBrain`)
  - **Slime** — стрибає до цілі (`SlimeHopBrain`)
- Отримання шкоди миттєво вмикає стан Chase, тому відкидання ніколи не «з'їдається»
- Детекція країв (`Physics2D.OverlapCircle`) розвертає патруль біля обривів

### 💰 Прогресія та UI
- Збір монет із постійним гаманцем (`PlayerWallet` на `PlayerPrefs`)
- HUD здоров'я і монет, екран смерті, головне меню

---

## 🛠️ Технології

| | |
|---|---|
| Рушій | Unity 6 (6000.4.0f1), Universal Render Pipeline (URP) |
| Мова | C# |
| Ввід | Unity Input System |
| Додатково | Шейдери ShaderLab / HLSL |

---

## 📂 Структура проєкту

```
Assets/_Project/Scripts/
├── Camera/           # CameraFollow
├── Combat/           # Health, DamageDealer, HitInfo, Hitstop, AttackHitbox
│   └── Interfaces/   # IDamageable, IAttacker, IHealth, IKnockbackReceiver
├── Enemy/            # EnemyController, EnemyHealth, контролери типів ворогів
│   ├── Brains/       # PatrolChaseBrain, SlimeHopBrain (патерн Strategy)
│   └── Interfaces/   # IEnemyBrain, IBrainHitNotifier
├── Items/            # Coin + ICollectible
├── Player/           # PlayerMotor, PlayerCombat, PlayerHealth, PlayerFacade,
│   │                 # PlayerWallet, PlayerService
│   └── Interfaces/   # IPlayerInput, IPlayerMotor, IPlayerControl
└── UI/               # HealthHud, CoinsHud, DeathScreen, MainMenuController
```

---

## 🏗️ Архітектурні рішення

| Патерн | Де | Навіщо |
|---|---|---|
| **Strategy** | AI ворогів (`IEnemyBrain`) | Поведінку ворога можна замінити, не чіпаючи контролери |
| **Template Method** | Система шкоди (`Health` → `PlayerHealth` / `EnemyHealth`) | Спільний каркас (невразливість, події, смерть), специфічні реакції — у нащадках |
| **Service Locator** | `PlayerService` | Пошук гравця за O(1), без `Find` кожного кадру |
| **DI через інтерфейси** | `PlayerMotor` ← `IPlayerInput` | Ввід можна підмінити на AI чи replay без змін коду |
| **Facade** | `PlayerFacade` | Єдина точка синхронізації Animator; інші компоненти лише піднімають параметри |

Повний розбір з обґрунтуванням кожного рішення: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 🚀 Як запустити

1. Встанови **Unity 6000.4.0f1** (або новіший Unity 6) через Unity Hub
2. Клонуй репозиторій:
   ```bash
   git clone https://github.com/San4o5/AshenForgotten.git
   ```
3. Відкрий теку проєкту в Unity Hub
4. Відкрий головну сцену з `Assets/Scenes/` та натисни **Play**

### 🎮 Керування

<!-- TODO: звір розкладку з InputSystem_Actions.inputactions -->
| Дія | Клавіша |
|---|---|
| Рух | `A` / `D` |
| Стрибок | `Space` (утримуй для вищого стрибка) |
| Атака | `ЛКМ` |

---

## 🚧 Статус

Гра в активній розробці. У планах:
<!-- TODO: підлаштуй roadmap під реальні плани -->
- [ ] Більше типів ворогів
- [ ] Бос-файт
- [ ] Система збереження прогресу рівнів
- [ ] Звук і музика

---

## 👤 Автор

**Олександр Бабарика** — [github.com/San4o5](https://github.com/San4o5)
