using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage      = 10f;
    [SerializeField] private float attackCooldown    = 0.5f;
    [SerializeField] private float hitboxActiveTime  = 0.15f;
    [SerializeField] private float attackRange       = 4f;
    [SerializeField] private float hitboxOffset      = 0.7f;

    [Header("Water Wave")]
    [SerializeField] private GameObject waveProjectilePrefab;
    [SerializeField] private float maxWaveChargeTime = 1.5f;
    [SerializeField] private float waveSpawnDistance = 0.6f;
    [SerializeField] private float smallWaveDamageMultiplier = 1f;
    [SerializeField] private float mediumWaveDamageMultiplier = 2f;
    [SerializeField] private float largeWaveDamageMultiplier = 3f;
    [SerializeField] private float smallWaveSpeed = 6f;
    [SerializeField] private float mediumWaveSpeed = 9f;
    [SerializeField] private float largeWaveSpeed = 12f;
    [SerializeField] private Vector3 smallWaveScale = new Vector3(0.35f, 0.35f, 1f);
    [SerializeField] private Vector3 mediumWaveScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 largeWaveScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Sprite smallWaveSprite;
    [SerializeField] private Sprite mediumWaveSprite;
    [SerializeField] private Sprite largeWaveSprite;

    [Header("References")]
    [Tooltip("Child GameObject with a Trigger Collider2D used as the melee hitbox.")]
    [SerializeField] private GameObject attackHitbox;

    [Tooltip("Optional Animator — triggers 'Attack' parameter.")]
    [SerializeField] private Animator animator;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float cooldownTimer  = 0f;
    private float hitboxTimer    = 0f;
    private bool  hitboxActive   = false;
    private bool isWaveCharging  = false;
    private float waveChargeTime = 0f;
    private MovementSystem movementSystem;
    private Rigidbody2D rb2d;
    private Vector2 lastMoveDirection = Vector2.right;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        movementSystem = GetComponent<MovementSystem>();
        rb2d = GetComponent<Rigidbody2D>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        HandleWaveKeyboardFallback();

        if (isWaveCharging)
        {
            if (!CanUseWaveAttack())
            {
                CancelWaveCharge();
            }
            else
            {
                waveChargeTime = Mathf.Min(waveChargeTime + Time.deltaTime, maxWaveChargeTime);
            }
        }

        if (hitboxActive)
        {
            hitboxTimer -= Time.deltaTime;
            if (hitboxTimer <= 0f)
            {
                hitboxActive = false;
                if (attackHitbox != null)
                    attackHitbox.SetActive(false);
            }
        }

        if (movementSystem != null)
        {
            Vector2 movementDirection = movementSystem.GetDirection();
            if (movementDirection.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = movementDirection.normalized;
                return;
            }
        }

        if (rb2d != null && rb2d.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = rb2d.linearVelocity.normalized;
        }
    }

    private void HandleWaveKeyboardFallback()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.qKey.wasPressedThisFrame)
            BeginWaveCharge();

        if (keyboard.qKey.wasReleasedThisFrame)
            ReleaseWaveCharge();
    }

    // -------------------------------------------------------------------------
    // Input System Callback (Send Messages)
    // -------------------------------------------------------------------------

    private void OnAttack(InputValue value)
    {
        if (!value.isPressed)      return;
        if (cooldownTimer > 0f)    return;

        PerformAttack();
    }

    private void OnWaveAbility(InputValue value)
    {
        if (value.isPressed)
        {
            BeginWaveCharge();
        }
        else
        {
            ReleaseWaveCharge();
        }
    }

    // -------------------------------------------------------------------------
    // Attack Logic
    // -------------------------------------------------------------------------

    private void PerformAttack()
    {
        cooldownTimer = attackCooldown;

        if (animator != null)
            animator.SetTrigger(AnimAttack);

        if (attackHitbox != null)
        {
            attackHitbox.transform.localPosition = lastMoveDirection * hitboxOffset;
            attackHitbox.SetActive(true);
            hitboxActive = true;
            hitboxTimer  = hitboxActiveTime;
        }
        else
        {
            FallbackOverlapAttack();
        }
    }

    // -------------------------------------------------------------------------
    // Hitbox Collision
    // -------------------------------------------------------------------------

    private void FallbackOverlapAttack()
    {
        // Attack in the direction the player is facing, not a circle around them
        Vector2 attackPos = (Vector2)transform.position + lastMoveDirection * hitboxOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange);

        // Only damage the closest enemy, not everything in range
        float closestDist = float.MaxValue;
        IDamageable closestTarget = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.transform.IsChildOf(transform)) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = damageable;
                }
            }
        }

        if (closestTarget != null)
            closestTarget.TakeDamage(attackDamage);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void OnHitboxTrigger(Collider2D other)
    {
        // Only damage things that are actually close to the player
        float dist = Vector2.Distance(transform.position, other.transform.position);
        if (dist > attackRange)
        {
            Debug.Log($"Ignored hit on {other.name} — too far away ({dist:F1} units)");
            return;
        }

        Debug.Log("Hit: " + other.name);

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(attackDamage);
    }

    public void BeginWaveCharge()
    {
        if (!CanUseWaveAttack())
            return;

        isWaveCharging = true;
        waveChargeTime = 0f;
    }

    public void ReleaseWaveCharge()
    {
        if (!isWaveCharging)
            return;

        float chargePercent = maxWaveChargeTime <= 0f ? 1f : waveChargeTime / maxWaveChargeTime;
        FireWave(chargePercent);
        CancelWaveCharge();
    }

    public void CancelWaveCharge()
    {
        isWaveCharging = false;
        waveChargeTime = 0f;
    }

    private bool CanUseWaveAttack()
    {
        if (GameStateManager.IsUIOpen)
            return false;

        return GameProgressManager.Instance != null &&
               GameProgressManager.Instance.HasElement(ElementType.Water);
    }

    private void FireWave(float chargePercent)
    {
        if (waveProjectilePrefab == null)
        {
            Debug.LogWarning("Wave projectile prefab is not assigned on PlayerCombat.");
            return;
        }

        Vector2 facingDirection = lastMoveDirection == Vector2.zero
            ? Vector2.right
            : lastMoveDirection.normalized;

        float selectedDamageMultiplier;
        float selectedSpeed;
        Vector3 selectedScale;
        Sprite selectedSprite;

        if (chargePercent < 0.33f)
        {
            selectedDamageMultiplier = smallWaveDamageMultiplier;
            selectedSpeed = smallWaveSpeed;
            selectedScale = smallWaveScale;
            selectedSprite = smallWaveSprite;
        }
        else if (chargePercent < 0.66f)
        {
            selectedDamageMultiplier = mediumWaveDamageMultiplier;
            selectedSpeed = mediumWaveSpeed;
            selectedScale = mediumWaveScale;
            selectedSprite = mediumWaveSprite;
        }
        else
        {
            selectedDamageMultiplier = largeWaveDamageMultiplier;
            selectedSpeed = largeWaveSpeed;
            selectedScale = largeWaveScale;
            selectedSprite = largeWaveSprite;
        }

        Vector3 spawnPosition = transform.position + (Vector3)(facingDirection * waveSpawnDistance);
        GameObject wave = Instantiate(waveProjectilePrefab, spawnPosition, Quaternion.identity);
        wave.transform.localScale = selectedScale;

        ChargedWaveProjectile projectile = wave.GetComponent<ChargedWaveProjectile>();
        if (projectile == null)
        {
            Debug.LogWarning("ChargedWaveProjectile component missing on wave projectile prefab.");
            Destroy(wave);
            return;
        }

        projectile.Initialize(
            attackDamage * selectedDamageMultiplier,
            selectedSpeed,
            selectedSprite,
            facingDirection,
            ElementType.Water);
    }

    /// <summary>
    /// Called by PlayerStats to enforce consistent stats across all levels.
    /// </summary>
    public void SetAttackDamage(float newDamage)
    {
        attackDamage = newDamage;
    }

    public float GetAttackRange() => attackRange;

    public void SetAttackRange(float newRange)
    {
        attackRange = newRange;

        AttackRangeIndicator indicator = GetComponent<AttackRangeIndicator>();
        if (indicator != null)
            indicator.SetRange(newRange);
    }

    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = newCooldown;
    }

    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }
    // -------------------------------------------------------------------------
    // Editor Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

