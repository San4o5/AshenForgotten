# Архітектура коду — Ashen Forgotten

Файлова мапа всіх скриптів у `Assets/_Project/Scripts/` з ключовими фрагментами і коротким описом. Доповнює CLAUDE.md (який про геймдизайн/стан проєкту) технічними деталями.

## Зміст

- [Загальні принципи](#загальні-принципи)
- [Combat/](#combat) — бойова механіка
- [Player/](#player) — гравець (5 SRP-компонентів)
- [Enemies/](#enemies) — базовий клас + Strategy (brains)
- [Items/](#items) — підбірне
- [Camera/](#camera) — камера

## Combat/

### `HitInfo.cs` — readonly struct, що передається через `TakeDamage`

```csharp
public readonly struct HitInfo
{
    public readonly int Damage;
    public readonly Vector2 SourcePosition;
    public readonly Vector2 Direction;
    public readonly float KnockbackForce;
    public readonly float KnockbackUp;
    public readonly GameObject Attacker;
    // ...
}
```

Замість простого `int damage` несе **повний контекст удару** — звідки, куди, яким імпульсом. Direction нормалізується у конструкторі.

### `Interfaces/IDamageable.cs`, `IHealth.cs`, `IAttacker.cs`, `IKnockbackReceiver.cs`

```csharp
public interface IDamageable
{
    bool IsDead { get; }
    void TakeDamage(HitInfo hit);
}
```

Decoupling: `DamageDealer` не знає, чи б'є він гравця чи ворога — лише `IDamageable`. `IKnockbackReceiver` — окремий інтерфейс, бо не всі цілі реагують на knockback.

### `Health.cs` — abstract base для PlayerHealth/EnemyHealth

```csharp
public abstract class Health : MonoBehaviour, IDamageable, IHealth
{
    public event Action<int, int> HealthChanged;
    public event Action Died;

    public void TakeDamage(HitInfo hit)
    {
        if (_isDead) return;
        if (_invincibleTimer > 0f) return;
        if (hit.Damage <= 0) return;

        _current = Mathf.Max(0, _current - hit.Damage);
        _invincibleTimer = _invincibilityDuration;
        HealthChanged?.Invoke(_current, _maxHealth);

        OnDamageReceived(hit);
        if (_current == 0) { _isDead = true; OnDie(hit); Died?.Invoke(); }
    }

    protected abstract void OnDamageReceived(in HitInfo hit);
    protected abstract void OnDie(in HitInfo hit);
}
```

Шаблонний метод: спільна логіка (HP, інвулнерабельність, події) тут. Конкретні реакції (анімації Hurt/Die, knockback) — у нащадках через `OnDamageReceived/OnDie`.

### `DamageDealer.cs` — реалізує `IAttacker`, генерує HitInfo

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
    if (other.isTrigger) return;  // ігнорувати hurtbox'и

    var target = other.GetComponentInParent<IDamageable>();
    if (target == null || target.IsDead) return;
    if (!_hitThisSwing.Add(target)) return;  // один удар = одна ціль

    var hit = new HitInfo(_damage, Position, dir, _knockbackForce, _knockbackUp, gameObject);
    target.TakeDamage(hit);
    // ...
}
```

Висить на `AttackHitbox` гравця. `_hitThisSwing` забезпечує що один змах меча не б'є одну ціль двічі. `ResetHits()` викликається при активації hitbox'а.

### `AttackHitbox.cs` — вмикає/вимикає колайдер через Animation Events

```csharp
public void Activate()    // викликається з HER_Attack animation event
{
    _damageDealer.ResetHits();
    _collider.enabled = true;
    if (_slashVfx != null) _slashVfx.enabled = true;
}
public void Deactivate()  // викликається в кінці анімації
```

Hitbox активний тільки під час "active frames" атаки. Контролюється з .anim файлу через Animation Events `OnAttackHit`/`OnAttackEnd` у `PlayerCombat`.

---

## Player/

### `Interfaces/IPlayerInput.cs`, `IPlayerMotor.cs`, `IPlayerControl.cs`

```csharp
public interface IPlayerInput
{
    float Horizontal { get; }
    bool RunHeld { get; }
    bool JumpPressed { get; }
    bool JumpHeld { get; }
    bool AttackPressed { get; }
}
```

Контракти між компонентами Player. Motor не знає звідки приходить input — лише `IPlayerInput`. Це дозволяє підмінити на AI/replay/мережевий input без правок Motor.

### `PlayerInput.cs` — реалізує `IPlayerInput`, читає `Keyboard.current`

```csharp
private void Update()
{
    if (!_enabled) { Clear(); return; }
    var kb = Keyboard.current;
    if (kb == null) { Clear(); return; }

    Horizontal = 0f;
    if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) Horizontal = -1f;
    if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) Horizontal = 1f;

    RunHeld       = kb.leftShiftKey.isPressed && Horizontal != 0f;
    JumpPressed   = kb.spaceKey.wasPressedThisFrame;
    JumpHeld      = kb.spaceKey.isPressed;
    AttackPressed = kb.xKey.wasPressedThisFrame;
}
```

Default keys: A/D + arrows = рух, Shift = run, Space = jump, X = attack. `SetEnabled(false)` обнуляє все (використовується на смерті/knockback lock).

### `PlayerMotor.cs` — kinematic фізика з CapsuleCast

```csharp
private void Awake()
{
    _rb.bodyType = RigidbodyType2D.Kinematic;
    _rb.gravityScale = 0f;
    _rb.freezeRotation = true;
    _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
}
```

**Ключове рішення**: Kinematic Rigidbody2D + ручні Physics2D.CapsuleCast замість Dynamic. Це усуває "липкі стіни", "гумовість" і непередбачуваний стрибок — стандарт для metroidvania (Hollow Knight, Celeste).

```csharp
// Coyote-time + jump-buffer
bool wantsJump = (_jumpBufferTimer > 0f) || _input.JumpHeld;
if (wantsJump && _coyoteTimer > 0f)
{
    _velocity.y = _jumpVelocity;
    _jumpBufferTimer = 0f;
    _coyoteTimer = 0f;
    _isGrounded = false;
    _isJumping = true;
}
```

**Coyote-time (0.1s)** — стрибок можливий через 0.1с після того, як зійшов з краю. **Jump-buffer (0.1s)** — Space натиснутий до приземлення спрацьовує одразу при приземленні.

```csharp
// Variable jump height — cut upward velocity ONCE on Space release
bool jumpReleased = _jumpHeldPrev && !_input.JumpHeld;
if (_isJumping && jumpReleased && _velocity.y > 0f)
{
    _velocity.y *= _jumpCutMultiplier;
    _isJumping = false;   // consume — never cut twice in one jump
}
```

**Jump-cut** — короткий тап Space → менший стрибок. `_isJumping` flag гарантує, що зрізання спрацьовує **рівно 1 раз** (раніше був баг — кожен кадр).

```csharp
private RaycastHit2D CapsuleCast(Vector2 direction, float distance)
{
    return Physics2D.CapsuleCast(origin, size, _capsule.direction, 0f,
                                  direction, distance, _groundLayer);
}
```

Окремий CapsuleCast для X і Y руху (MoveX, MoveY). Маска `_groundLayer` — гравець стикається тільки з землею (не з ворогами).

### `PlayerCombat.cs` — атака + керування AttackHitbox через Animation Events

```csharp
private void Update()
{
    if (!_enabled) return;
    if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

    if (!_input.AttackPressed) return;
    if (_attackTimer > 0f) return;

    _attackTimer = _attackCooldown;
    if (_animator != null) _animator.SetTrigger(HashAttack);
}

// Called from HER_Attack animation event at active frame
public void OnAttackHit() => _attackHitbox?.Activate();
public void OnAttackEnd() => _attackHitbox?.Deactivate();
```

Натискання X → анімація Attack. На "active frame" анімації Unity викликає `OnAttackHit` — hitbox вмикається. На останньому кадрі — `OnAttackEnd`.

### `PlayerHealth.cs` — наслідує `Health`, реалізує `IKnockbackReceiver`

```csharp
protected override void OnDamageReceived(in HitInfo hit)
{
    ApplyKnockback(in hit);
    if (_animator != null) _animator.SetTrigger(HashHurt);
}

public void ApplyKnockback(in HitInfo hit)
{
    float force = hit.KnockbackForce > 0f ? hit.KnockbackForce : _defaultKnockbackForce;
    float up    = hit.KnockbackUp    > 0f ? hit.KnockbackUp    : _defaultKnockbackUp;

    float dirX = Mathf.Sign(transform.position.x - hit.SourcePosition.x);
    if (dirX == 0f) dirX = 1f;

    _motor.ApplyExternalVelocity(new Vector2(dirX * force, up));
    _control?.LockControl(_knockbackLockTime);
}
```

Якщо HitInfo несе knockback — використовуємо його. Інакше дефолт `(_defaultKnockbackForce=8, _defaultKnockbackUp=5)`. Напрямок — **від атакуючого** (Source) у бік гравця. `LockControl(0.2s)` блокує input на час knockback'а.

### `PlayerFacade.cs` — координатор: animator, PlayerService registry, control lock

```csharp
private void OnEnable()
{
    PlayerService.Register(transform, _health, _health);
}

private void UpdateAnimator()
{
    _animator.SetFloat(HashSpeed, Mathf.Abs(_input.Horizontal));
    _animator.SetBool(HashIsRunning, _input.RunHeld);
    _animator.SetBool(HashIsGrounded, _motor.IsGrounded);
    _animator.SetFloat(HashVerticalVelocity, _motor.Velocity.y);
}

public void LockControl(float duration)
{
    _controlLockTimer = Mathf.Max(_controlLockTimer, duration);
    ApplyControlState();
}
```

Єдина точка зв'язку Player-компонентів з Animator (інші компоненти лише тригерять trigger-параметри). Реєструє у `PlayerService` обидва інтерфейси з PlayerHealth (`IDamageable` + `IHealth`).

### `PlayerDebugTools.cs` — H-клавіша self-damage у Editor

```csharp
#if UNITY_EDITOR
private void Update()
{
    if (kb.hKey.wasPressedThisFrame)
    {
        float dirX = _motor.FacingRight ? 1f : -1f;
        Vector2 source = (Vector2)transform.position + new Vector2(dirX, 0f);
        _health.TakeDamage(new HitInfo(_selfDamage, source, Vector2.right * -dirX));
    }
}
#endif
```

Тестування Hurt/Die анімацій без ворогів. Цілком вирізається з білду через `#if UNITY_EDITOR`.

### `PlayerService.cs` — статичний registry

```csharp
public static class PlayerService
{
    public static Transform PlayerTransform { get; private set; }
    public static IDamageable PlayerDamageable { get; private set; }
    public static IHealth PlayerHealth { get; private set; }

    public static void Register(Transform t, IDamageable damageable, IHealth health) { ... }
    public static void Unregister(Transform t) { ... }
}
```

Замінює `FindGameObjectWithTag("Player")` — O(1) доступ, без щокадрного пошуку. Вороги шукають гравця через `PlayerService.PlayerTransform`. Реєстрація в `PlayerFacade.OnEnable`, відписка в `OnDisable`.

### `PlayerWallet.cs` — монети + PlayerPrefs persistence

```csharp
public static class PlayerWallet
{
    public static int Coins { get; private set; }
    public static event Action<int> CoinsChanged;

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        if (!_loaded) Load();
        Coins += amount;
        Save();
        CoinsChanged?.Invoke(Coins);
    }

    private static void Save() => PlayerPrefs.SetInt(KeyCoins, Coins);
}
```

Static — глобальний стан, доступний звідусіль. Подія `CoinsChanged` для майбутнього UI Coin counter. `_loaded` flag з lazy `Load()` — safety net якщо `AddCoins` викликають до `Bootstrap.Awake()`.

### `PlayerWalletBootstrap.cs` — `Awake → PlayerWallet.Load()`

```csharp
public class PlayerWalletBootstrap : MonoBehaviour
{
    private void Awake() => PlayerWallet.Load();
}
```

Завантажує збережені монети на старті сцени. Прикріплений до Player.

---

## Enemies/

### `Interfaces/IEnemyBrain.cs` — Strategy pattern + EnemyContext + IEnemyView

```csharp
public interface IEnemyBrain
{
    void Init(EnemyContext ctx);
    void Tick(float dt);
    void OnDamaged(in HitInfo hit);
}

public sealed class EnemyContext
{
    public Transform Self;
    public Rigidbody2D Body;
    public Transform Player;
    public Transform EdgeCheck;
    public LayerMask GroundLayer;
    public IEnemyView View;
}

public interface IEnemyView
{
    bool FacingRight { get; }
    void SetFacing(float dir);
}
```

**Strategy pattern**: різні вороги (Ash, Slime) реалізують різну поведінку через окремі Brain-класи. EnemyController створює конкретний Brain через абстрактний `CreateBrain()`. EnemyContext — DTO з усім, що brain'у потрібно (без зв'язку з конкретним контролером).

### `Interfaces/IBrainHitNotifier.cs`

```csharp
public interface IBrainHitNotifier
{
    void NotifyBrainOfHit(in HitInfo hit);
}
```

Дозволяє `DamageDealer` повідомити Brain про hit — для aggro-реакції (наприклад, перехід у Chase).

### `EnemyController.cs` — abstract base

```csharp
public abstract class EnemyController : MonoBehaviour, IEnemyView, IBrainHitNotifier
{
    protected virtual void Start()
    {
        _ctx = new EnemyContext { Self = transform, Body = _rb, /* ... */ };
        _brain = CreateBrain();
        _brain.Init(_ctx);
    }

    protected virtual void FixedUpdate()
    {
        if (_health.IsDead) { _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); return; }
        _brain?.Tick(Time.fixedDeltaTime);
        SetSpeed(Mathf.Abs(_rb.linearVelocity.x));
    }

    protected abstract IEnemyBrain CreateBrain();

    public void NotifyBrainOfHit(in HitInfo hit) => _brain?.OnDamaged(in hit);
}
```

Спільна логіка для всіх ворогів: RB, Animator, Health, цикл Tick, facing, animator's Speed param. Нащадки реалізують лише **який** Brain створити.

### `EnemyHealth.cs` — наслідує `Health`, додає knockback + destroy delay

```csharp
protected override void OnDamageReceived(in HitInfo hit)
{
    if (_animator != null) _animator.SetTrigger(HashHurt);

    if (_rb != null && _rb.bodyType == RigidbodyType2D.Dynamic && _hurtKnockbackForce > 0f)
    {
        float dirX = Mathf.Sign(transform.position.x - hit.SourcePosition.x);
        if (dirX == 0f) dirX = hit.Direction.x >= 0f ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dirX * _hurtKnockbackForce, _hurtKnockbackUp);
    }
}

protected override void OnDie(in HitInfo hit)
{
    if (_animator != null) _animator.SetTrigger(HashDie);
    if (_rb != null) _rb.linearVelocity = Vector2.zero;
    foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
    Destroy(gameObject, _destroyDelayAfterDie);
}
```

Knockback застосовується **прямо до Rigidbody2D** (вороги Dynamic, на відміну від Kinematic Player). Якщо `_hurtKnockbackForce=0` — ворог "immune" до knockback (наприклад, orange slime). На смерті — вимикаємо всі колайдери (не пропускає удари) і Destroy через delay (час догратись Die анімації).

### `EnemyDamageOnTouch.cs` — контактний урон через trigger-зону

```csharp
public Vector2 Position => transform.parent != null
    ? (Vector2)transform.parent.position
    : (Vector2)transform.position;

private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

private void TryDamage(Collider2D other)
{
    if (Time.time - _lastHitTime < _hitCooldown) return;
    if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
    if (other.isTrigger) return;

    var target = other.GetComponentInParent<IDamageable>();
    if (target == null || target.IsDead) return;

    var hit = new HitInfo(_damage, Position, dir, _knockbackForce, _knockbackUp, gameObject);
    target.TakeDamage(hit);
    _lastHitTime = Time.time;
}
```

**Висить на child `HurtZone`** (trigger), а не на root. Це обхід обмеження Unity2D — `OnCollisionStay2D` не доставляється між Kinematic (Player) і Dynamic (enemy). `Position` повертає батьківську (root) позицію — knockback напрямлений від тіла ворога, не від точки тригера.

### `AshServantController.cs` — створює `PatrolChaseBrain`

```csharp
public class AshServantController : EnemyController
{
    protected override IEnemyBrain CreateBrain()
    {
        return new PatrolChaseBrain(
            _patrolSpeed, _patrolRange,
            _detectionRange, _chaseSpeed, _loseTargetRange,
            _edgeCheckRadius);
    }
}
```

Лише налаштування + вибір стратегії. Решта логіки в EnemyController + PatrolChaseBrain.

### `SlimeController.cs` — створює `SlimeHopBrain`

```csharp
public class SlimeController : EnemyController
{
    protected override IEnemyBrain CreateBrain()
    {
        return new SlimeHopBrain(
            _hopHorizontalSpeed, _hopVerticalImpulse,
            _minIdleTime, _maxIdleTime,
            _patrolRange, _detectionRange, _loseTargetRange,
            _groundCheckOffsetY);
    }
}
```

Те ж саме що Ash, але з SlimeHopBrain. Параметри hop'а (швидкість, висота, idle time) серіалізовані.

### `Brains/PatrolChaseBrain.cs` — двостанова FSM (Patrol ↔ Chase)

```csharp
public void Tick(float dt)
{
    UpdateState();
    switch (_state)
    {
        case State.Patrol: TickPatrol(); break;
        case State.Chase:  TickChase();  break;
    }
}

private void UpdateState()
{
    float dist = Vector2.Distance(_ctx.Self.position, _ctx.Player.position);
    if (_state == State.Patrol && dist <= _detectionRange) _state = State.Chase;
    else if (_state == State.Chase && dist >= _loseTargetRange) _state = State.Patrol;
}

public void OnDamaged(in HitInfo hit) => _state = State.Chase;
```

Patrol = ходить туди-сюди в межах `_patrolRange` від origin. Chase = рухається до гравця з `_chaseSpeed`. Hysteresis (`detectionRange < loseTargetRange`) щоб не "блимав" туди-сюди на границі. **Hit → одразу Chase** (aggro).

```csharp
private void TickPatrol()
{
    float dxFromOrigin = _ctx.Self.position.x - _origin.x;
    if (dxFromOrigin > _patrolRange && _facingDir > 0f) Flip(-1f);
    else if (dxFromOrigin < -_patrolRange && _facingDir < 0f) Flip(1f);

    if (!HasGroundAhead()) Flip(-_facingDir);

    _ctx.Body.linearVelocity = new Vector2(_facingDir * _patrolSpeed, _ctx.Body.linearVelocity.y);
}
```

Розвертається на границях патрулю + на краях платформи (через `EdgeCheck` child з `Physics2D.OverlapCircle`).

### `Brains/SlimeHopBrain.cs` — трирівнева FSM (WaitOnGround → Hop → Airborne)

```csharp
private enum State { WaitOnGround, Hop, Airborne }
private enum Mode { Patrol, Chase }

public void Tick(float dt)
{
    UpdateMode();
    switch (_state)
    {
        case State.WaitOnGround: TickWait(dt); break;
        case State.Hop:          TickHop();    break;
        case State.Airborne:     TickAir();    break;
    }
}
```

**State** — фізична фаза, **Mode** — намір (патруль/переслідування). На стрибку: дочекатися idle timer → Hop (приклад velocity) → Airborne (чекати приземлення) → WaitOnGround знов.

```csharp
public void OnDamaged(in HitInfo hit)
{
    _mode = Mode.Chase;
    _state = State.Airborne;  // не дати TickWait обнулити knockback velocity
}
```

**Важлива деталь**: коли слайма б'ють у стані WaitOnGround, brain перемикається у Airborne — інакше `TickWait` в наступному кадрі занулив би velocity.x і знищив knockback.

```csharp
private float ChooseHopDirection()
{
    if (_mode == Mode.Chase && _ctx.Player != null)
    {
        float d = Mathf.Sign(_ctx.Player.position.x - _ctx.Self.position.x);
        return d == 0f ? _facingDir : d;
    }
    // Patrol: reverse at bounds
    float dxFromOrigin = _ctx.Self.position.x - _origin.x;
    if (dxFromOrigin >  _patrolRange) return -1f;
    if (dxFromOrigin < -_patrolRange) return  1f;
    return _facingDir;
}
```

Напрямок стрибка: до гравця у Chase, до origin якщо вийшов за патрульну межу, інакше — той же бік куди й дивиться.

```csharp
private bool IsGrounded()
{
    Vector2 origin = (Vector2)_ctx.Self.position + Vector2.down * _groundCheckOffsetY;
    var hit = Physics2D.Raycast(origin, Vector2.down, 0.15f, _ctx.GroundLayer);
    return hit.collider != null;
}
```

Простий raycast вниз з ніг — перевірка приземлення в стані Airborne.

---

## Items/

### `Interfaces/ICollectible.cs`

```csharp
public interface ICollectible
{
    void OnCollect(GameObject collector);
}
```

Базовий контракт для предметів (зараз тільки Coin реалізує).

### `Coin.cs` — підбірна монета

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (_collected) return;
    if (!other.CompareTag(_playerTag)) return;
    OnCollect(other.gameObject);
}

public void OnCollect(GameObject collector)
{
    if (_collected) return;
    _collected = true;

    PlayerWallet.AddCoins(_value);
    Destroy(gameObject);
}
```

`_collected` flag запобігає подвійному зарахуванню (наприклад, якщо два колайдери гравця заходять одночасно). `PlayerWallet.AddCoins` зберігає у PlayerPrefs.

---

## Camera/

### `CameraFollow.cs` — dead zone + SmoothDamp

```csharp
private void LateUpdate()
{
    Vector3 targetPos = _target.position + new Vector3(0f, _offsetY, 0f);

    float halfW = _deadZoneWidth * 0.5f;
    float halfH = _deadZoneHeight * 0.5f;

    if (targetPos.x > _desiredPosition.x + halfW) _desiredPosition.x = targetPos.x - halfW;
    else if (targetPos.x < _desiredPosition.x - halfW) _desiredPosition.x = targetPos.x + halfW;

    if (targetPos.y > _desiredPosition.y + halfH) _desiredPosition.y = targetPos.y - halfH;
    else if (targetPos.y < _desiredPosition.y - halfH) _desiredPosition.y = targetPos.y + halfH;

    Vector3 desired = new Vector3(_desiredPosition.x, _desiredPosition.y, transform.position.z);
    transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime, _maxSpeed);
}
```

**Dead zone** — прямокутник навколо камери, у межах якого гравець не рухає камеру. Виходить за межі → камера підтягується. **SmoothDamp** замість Lerp бо framerate-незалежний. `LateUpdate` — щоб камера рухалась **після** того як гравець порухався у Update.

---

## Як додавати нове

### Новий тип ворога

1. Створи `<Name>Controller.cs : EnemyController` з `CreateBrain()` що повертає твій brain.
2. Якщо потрібна нова поведінка — створи `<Name>Brain.cs : IEnemyBrain` у `Enemies/Brains/`.
3. Якщо потрібна нова інформація для brain — додай поле в `EnemyContext`.
4. Створи prefab: RB2D Dynamic, CapsuleCollider2D (solid), EnemyHealth, твій Controller. Child `HurtZone` з trigger CapsuleCollider2D + EnemyDamageOnTouch.

### Новий тип атаки гравця

1. Додай поле в `IPlayerInput` (напр. `bool HeavyAttackPressed`).
2. Реалізуй у `PlayerInput.cs`.
3. У `PlayerCombat` додай логіку + новий Animation Event handler.
4. Або створи окремий компонент `PlayerHeavyAttack.cs` за SRP.

### Новий тип предмета

1. Створи компонент що реалізує `ICollectible`.
2. Якщо потрібен новий "гаманець" (наприклад HealthPotions) — створи статичний `PlayerInventory.cs` за зразком `PlayerWallet`.

---

## Pitfalls / тонкощі

- **Animation Events викликають методи за іменем (string)**. Якщо перейменуєш метод (наприклад `OnAttackHit`) — Animation Event у .anim **не оновиться автоматично**. Тримай імена стабільними або правь у .anim.
- **`Kinematic` Player + `Dynamic` enemy не доставляють `OnCollisionStay2D`** один одному. Тому contact-damage висить на child HurtZone (trigger), не на root.
- **Physics2D Layer Collision Matrix**: `Player ↔ Enemy = false`. Гравець фізично не стикається з ворогами (немає push-bug). HurtZone на шарі **Default** (не Enemy), тому Player ↔ Default = true → trigger damage продовжує спрацьовувати. Налаштування у `Edit > Project Settings > Physics 2D > Layer Collision Matrix`.
- **PlayerService.IsAvailable** — `null` при перших кадрах перед `OnEnable` PlayerFacade. Вороги перевіряють у FixedUpdate.
- **PlayerPrefs зберігається на диск тільки після `PlayerPrefs.Save()`**. Звичайний `SetInt` тримає у пам'яті.
- **`linearVelocity` (Unity 6) — нове ім'я для `velocity`** у Rigidbody2D. Старий `velocity` deprecated.
