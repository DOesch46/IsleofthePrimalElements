using UnityEngine;
using System.Collections;

/// <summary>
/// Controls Terradon, the Earth General boss.
///
/// SETUP IN UNITY:
/// 1. Create a GameObject named "Terradon" in your EarthLevel scene.
/// 2. Add a Rigidbody2D (Gravity Scale = 0, Freeze Rotation Z = checked).
/// 3. Add a Collider2D (e.g. CircleCollider2D) for physics / player hits.
/// 4. Attach this script.
/// 5. Assign rockPrefab (use FallingRock.prefab), playerTransform (drag the Player),
///    and optionally a bossHealthBarUI object.
/// 6. Create a "FallingRock" prefab using the FallingRock.cs script.
///
/// PHASES:
///   Normal  (HP > 50%) — slow, predictable attacks.
///   Enraged (HP <= 50%) — faster movement, shorter cooldowns, double rock throw.
///
/// ATTACKS:
///   Rock Slam   — spawns a ring of rocks around Terradon.
///   Rock Throw  — aims a boulder directly at the player.
///   Stomp       — charges up, then releases a shockwave knockback.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TerradonController : MonoBehaviour, IDamageable
{
    // -------------------------------------------------------------------------
    // Inspector — Stats
    // -------------------------------------------------------------------------

    [Header("Boss Stats")]
    [SerializeField] private float maxHealth        = 300f;
    [SerializeField] private float moveSpeed        = 1.2f;
    [SerializeField] private float enragedSpeedMult = 1.6f;   // speed multiplier when enraged
    [SerializeField] private float detectionRange   = 12f;    // how far Terradon can see the player
    [SerializeField] private float meleeRange       = 1.2f;   // stop moving when this close

    [Header("Attack — Rock Throw")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float rockThrowCooldown        = 3f;
    [SerializeField] private float rockThrowCooldownEnraged = 1.8f;
    [SerializeField] private int   rocksPerThrowEnraged     = 2;   // extra rocks when enraged
    [SerializeField] private float rockSpread               = 20f; // spread angle for enraged multi-throw

    [Header("Attack — Rock Slam")]
    [SerializeField] private float slamCooldown        = 6f;
    [SerializeField] private float slamCooldownEnraged = 3.5f;
    [SerializeField] private int   slamRockCount       = 8;        // rocks spawned in ring
    [SerializeField] private float slamRingRadius      = 2.5f;

    [Header("Attack — Stomp Shockwave")]
    [SerializeField] private float stompCooldown        = 8f;
    [SerializeField] private float stompCooldownEnraged = 5f;
    [SerializeField] private float stompChargeTime      = 1.2f;    // windup duration
    [SerializeField] private float stompKnockbackForce  = 10f;
    [SerializeField] private float stompRadius          = 3f;

    [Header("References")]
    [SerializeField] private Transform   playerTransform;
    [SerializeField] private GameObject  bossHealthBarUI;   // optional — drag BossHealthBarUI here
    [SerializeField] private Transform   rockSpawnPoint;    // where rocks spawn; defaults to self
    [SerializeField] private GameObject  earthItemPrefab;   // dropped on death

    [Header("Screen Shake (optional)")]
    [SerializeField] private float deathShakeDuration  = 0.6f;
    [SerializeField] private float deathShakeMagnitude = 0.3f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D   rb;
    private Animator      anim;
    private float         currentHealth;
    private bool          isEnraged    = false;
    private bool          isDead       = false;
    private bool          isAttacking  = false;

    // individual attack timers
    private float throwTimer = 0f;
    private float slamTimer  = 0f;
    private float stompTimer = 0f;

    // Animator parameter hashes
    private static readonly int AnimWalk    = Animator.StringToHash("IsWalking");
    private static readonly int AnimSlam    = Animator.StringToHash("RockSlam");
    private static readonly int AnimThrow   = Animator.StringToHash("RockThrow");
    private static readonly int AnimStomp   = Animator.StringToHash("Stomp");
    private static readonly int AnimHit     = Animator.StringToHash("Hit");
    private static readonly int AnimDead    = Animator.StringToHash("Dead");
    private static readonly int AnimEnraged = Animator.StringToHash("Enraged");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // optional — works without an Animator

        rb.gravityScale  = 0f;
        rb.freezeRotation = true;

        currentHealth = maxHealth;

        // Stagger attack timers so they don't all fire at once
        throwTimer = rockThrowCooldown * 0.4f;
        slamTimer  = slamCooldown  * 0.7f;
        stompTimer = stompCooldown;
    }

    private void Start()
    {
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(true);

        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning("TerradonController: No Player found. Tag your player 'Player' or assign playerTransform.");
        }

        if (rockSpawnPoint == null)
            rockSpawnPoint = transform;
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer > detectionRange)
        {
            Idle();
            return;
        }

        TickAttackTimers();

        if (!isAttacking)
        {
            TryAttack(distToPlayer);

            if (!isAttacking)
                MoveTowardPlayer(distToPlayer);
        }

        CheckEnrageThreshold();
        FacePlayer();
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MoveTowardPlayer(float distToPlayer)
    {
        if (distToPlayer <= meleeRange)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool(AnimWalk, false);
            return;
        }

        float speed = moveSpeed * (isEnraged ? enragedSpeedMult : 1f);
        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
        anim?.SetBool(AnimWalk, true);
    }

    private void Idle()
    {
        rb.linearVelocity = Vector2.zero;
        anim?.SetBool(AnimWalk, false);
    }

    /// <summary>Flip sprite to face the player horizontally.</summary>
    private void FacePlayer()
    {
        if (playerTransform == null) return;
        float dir = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(dir) > 0.05f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dir);
            transform.localScale = s;
        }
    }

    // -------------------------------------------------------------------------
    // Attack Timing
    // -------------------------------------------------------------------------

    private void TickAttackTimers()
    {
        throwTimer -= Time.deltaTime;
        slamTimer  -= Time.deltaTime;
        stompTimer -= Time.deltaTime;
    }

    private void TryAttack(float distToPlayer)
    {
        // Priority: Stomp > Slam > Throw
        float curStompCD = isEnraged ? stompCooldownEnraged : stompCooldown;
        float curSlamCD  = isEnraged ? slamCooldownEnraged  : slamCooldown;
        float curThrowCD = isEnraged ? rockThrowCooldownEnraged : rockThrowCooldown;

        if (stompTimer <= 0f && distToPlayer <= stompRadius * 1.5f)
        {
            stompTimer = curStompCD;
            StartCoroutine(StompAttack());
        }
        else if (slamTimer <= 0f)
        {
            slamTimer = curSlamCD;
            StartCoroutine(RockSlamAttack());
        }
        else if (throwTimer <= 0f)
        {
            throwTimer = curThrowCD;
            StartCoroutine(RockThrowAttack());
        }
    }

    // -------------------------------------------------------------------------
    // Attack Coroutines
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rock Throw — lobs one (or two when enraged) boulders at the player.
    /// </summary>
    private IEnumerator RockThrowAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim?.SetTrigger(AnimThrow);

        // Brief windup
        yield return new WaitForSeconds(0.5f);

        if (!isDead)
        {
            ThrowRockAtPlayer(0f);

            if (isEnraged)
            {
                yield return new WaitForSeconds(0.2f);
                // Throw a second rock with a slight angle spread
                ThrowRockAtPlayer(rockSpread);
            }
        }

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    /// <summary>
    /// Rock Slam — spawns a ring of rocks that fly outward from Terradon.
    /// </summary>
    private IEnumerator RockSlamAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim?.SetTrigger(AnimSlam);

        yield return new WaitForSeconds(0.6f); // windup

        if (!isDead)
            SpawnRockRing();

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    /// <summary>
    /// Stomp Shockwave — charges up with a visible pause, then knocks the player back.
    /// </summary>
    private IEnumerator StompAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim?.SetTrigger(AnimStomp);

        // Visible charge — Terradon stands still
        yield return new WaitForSeconds(stompChargeTime);

        if (!isDead)
            ReleaseShockwave();

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    // -------------------------------------------------------------------------
    // Attack Helpers
    // -------------------------------------------------------------------------

    /// <summary>Instantiates a FallingRock aimed at the player with an optional angle offset.</summary>
    private void ThrowRockAtPlayer(float angleOffset)
    {
        if (rockPrefab == null || playerTransform == null) return;

        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)rockSpawnPoint.position).normalized;

        // Rotate direction by the angle offset
        if (Mathf.Abs(angleOffset) > 0.01f)
        {
            float rad = angleOffset * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            dir = new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
        }

        // Target position: where the player currently is
        Vector2 targetPos = playerTransform.position;

        GameObject rock = Instantiate(rockPrefab, rockSpawnPoint.position, Quaternion.identity);
        FallingRock fr = rock.GetComponent<FallingRock>();
        if (fr != null)
            fr.Launch(targetPos);
    }

    /// <summary>Spawns rocks equally spaced in a ring, all flying outward.</summary>
    private void SpawnRockRing()
    {
        if (rockPrefab == null) return;

        float angleStep = 360f / slamRockCount;

        for (int i = 0; i < slamRockCount; i++)
        {
            float angle    = i * angleStep * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * slamRingRadius;
            Vector2 target = (Vector2)transform.position + offset * 3f; // fly outward

            GameObject rock = Instantiate(rockPrefab, (Vector2)transform.position + offset * 0.5f, Quaternion.identity);
            FallingRock fr = rock.GetComponent<FallingRock>();
            if (fr != null)
                fr.Launch(target);
        }
    }

    /// <summary>Applies knockback to the player if they are within stomp radius.</summary>
    private void ReleaseShockwave()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > stompRadius) return;

        // Knock the player back
        Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRB != null)
        {
            Vector2 knockDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            playerRB.AddForce(knockDir * stompKnockbackForce, ForceMode2D.Impulse);
        }

        // Deal a small amount of stomp damage
        PlayerHealth ph = playerTransform.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(10f);

        // Optional: spawn some rocks near the shockwave edge
        if (rockPrefab != null)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle  = i * 90f * Mathf.Deg2Rad;
                Vector2 spawnPos = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * stompRadius;
                Vector2 targetPos = spawnPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
                GameObject rock = Instantiate(rockPrefab, spawnPos, Quaternion.identity);
                FallingRock fr = rock.GetComponent<FallingRock>();
                if (fr != null)
                    fr.Launch(targetPos);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Enrage
    // -------------------------------------------------------------------------

    private void CheckEnrageThreshold()
    {
        if (!isEnraged && currentHealth <= maxHealth * 0.5f)
        {
            isEnraged = true;
            anim?.SetBool(AnimEnraged, true);
            Debug.Log("Terradon ENRAGED!");
            // You could trigger a visual effect or screen flash here
        }
    }

    // -------------------------------------------------------------------------
    // Damage & Death
    // -------------------------------------------------------------------------

    /// <summary>
    /// Call this when the player's attack hits Terradon.
    /// Hook this up via your PlayerCombat script, or by adding an
    /// OnTriggerEnter2D on a separate HitboxCollider child of the player.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(currentHealth, 0f);

        anim?.SetTrigger(AnimHit);

        // Health bar UI — hook this up once you have BossHealthBarUI built
        // bossHealthBarUI.GetComponent<BossHealthBarUI>().SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            StartCoroutine(Die());
    }

    /// <summary>Returns current health as 0–1 fraction (useful for health bar).</summary>
    public float GetHealthFraction() => currentHealth / maxHealth;

    private IEnumerator Die()
    {
        isDead       = true;
        isAttacking  = false;
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger(AnimDead);

        // Optional screen shake
        if (Camera.main != null)
            StartCoroutine(ScreenShake(deathShakeDuration, deathShakeMagnitude));

        // Wait for death animation
        yield return new WaitForSeconds(1.5f);

        // Drop elemental item reward
        if (earthItemPrefab != null)
            Instantiate(earthItemPrefab, transform.position, Quaternion.identity);

        // Hide boss health bar
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);

        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Screen Shake Utility
    // -------------------------------------------------------------------------

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    // -------------------------------------------------------------------------
    // Editor Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Stomp radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stompRadius);

        // Rock slam ring
        Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, slamRingRadius);

        // Melee stop range
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
