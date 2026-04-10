using UnityEngine;
using System.Collections;

public class EarthBossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth bossHealth;  // ✅ Changed from EarthBossHealth
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Ground Slam")]
    [SerializeField] private GameObject groundSlamPrefab;
    [SerializeField] private Sprite waveSprite;
    [SerializeField] private float slamDamage = 25f;
    [SerializeField] private float waveSpeed = 6f;
    [SerializeField] private float slamCooldown = 3f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float stopRange = 3f;

    [Header("Rendering — IMPORTANT")]
    [SerializeField] private string sortingLayerName = "Enemies";
    [SerializeField] private int sortingOrder = 5;

    [Header("Phase 2")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private int phase2SlamCount = 3;
    [SerializeField] private float phase2SlamSpread = 30f;

    [Header("Rock Rain")]
    [SerializeField] private FallingRockSpawner rockSpawner;

    [Header("Death Reward")]
    [SerializeField] private GameObject playerObject;

    private Transform playerTransform;
    private bool isAttacking = false;
    private float slamTimer = 0f;
    private bool isPhase2 = false;
    private bool isDead = false;

    private enum BossState { Idle, Chase, Attack, Stunned, Dead }
    private BossState currentState = BossState.Idle;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Enemies";
            spriteRenderer.sortingOrder = 6;
        }

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.sortingLayerName = "Enemies";
            sr.sortingOrder = 6;
        }
    }

    private void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<EnemyHealth>();  // ✅ Changed
        if (animator == null)
            animator = GetComponent<Animator>();

        if (bossHealth != null)
        {
            bossHealth.OnDied += HandleDeath;  // ✅ Uses EnemyHealth.OnDied
            bossHealth.OnHealthChanged += CheckPhaseTransition;
        }
        else
        {
            Debug.LogError("EarthBossAI: No EnemyHealth found on this GameObject!");
        }

        // Find player
        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerTransform = playerObject.transform;

        currentState = BossState.Idle;
        slamTimer = slamCooldown;

        PlayAnim("Idle");
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        slamTimer -= Time.deltaTime;
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case BossState.Idle:
                PlayAnim("Idle");
                if (distToPlayer < chaseRange)
                    currentState = BossState.Chase;
                break;

            case BossState.Chase:
                ChasePlayer(distToPlayer);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Animation Helper
    // -------------------------------------------------------------------------

    private void PlayAnim(string stateName)
    {
        if (animator == null) return;
        animator.CrossFade(stateName, 0f, 0);
    }

    // -------------------------------------------------------------------------
    // AI Logic
    // -------------------------------------------------------------------------

    private void ChasePlayer(float distance)
    {
        if (isAttacking) return;

        if (distance <= attackRange && slamTimer <= 0f)
        {
            currentState = BossState.Attack;
            StartCoroutine(PerformGroundSlam());
            return;
        }

        if (distance > stopRange)
        {
            PlayAnim("Walk");

            Vector2 dirToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)dirToPlayer * moveSpeed * Time.deltaTime;

            if (spriteRenderer != null)
                spriteRenderer.flipX = dirToPlayer.x < 0;
        }
        else
        {
            PlayAnim("Idle");
        }
    }

    private IEnumerator PerformGroundSlam()
    {
        isAttacking = true;
        PlayAnim("Attack");

        Vector3 originalPos = transform.position;
        float windUpTime = 0.5f;
        float elapsed = 0f;
        Vector3 raisedPos = originalPos + Vector3.up * 0.3f;

        while (elapsed < windUpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpTime;
            transform.position = Vector3.Lerp(originalPos, raisedPos, t);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(t * 4f, 1f));
            yield return null;
        }

        elapsed = 0f;
        float slamDownTime = 0.1f;
        while (elapsed < slamDownTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(raisedPos, originalPos, elapsed / slamDownTime);
            yield return null;
        }

        transform.position = originalPos;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        Vector2 slamDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        if (isPhase2)
        {
            for (int i = 0; i < phase2SlamCount; i++)
            {
                float angleOffset = (i - (phase2SlamCount - 1) / 2f) * phase2SlamSpread;
                Vector2 rotatedDir = RotateVector2(slamDirection, angleOffset);
                SpawnSlamWave(rotatedDir);
            }
        }
        else
        {
            SpawnSlamWave(slamDirection);
        }

        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.3f, 0.15f);

        slamTimer = isPhase2 ? slamCooldown * 0.7f : slamCooldown;

        yield return new WaitForSeconds(0.8f);

        isAttacking = false;
        currentState = BossState.Chase;
    }

    private void SpawnSlamWave(Vector2 direction)
    {
        if (groundSlamPrefab == null)
        {
            Debug.LogError("EarthBossAI: groundSlamPrefab is not assigned!");
            return;
        }

        Vector2 spawnPos = (Vector2)transform.position + direction * 1.0f;
        GameObject waveObj = Instantiate(groundSlamPrefab);
        GroundSlamWave wave = waveObj.GetComponent<GroundSlamWave>();

        if (wave == null)
        {
            Debug.LogError("EarthBossAI: groundSlamPrefab does not have a GroundSlamWave component!");
            Destroy(waveObj);
            return;
        }

        float currentWaveSpeed = isPhase2 ? waveSpeed * 1.3f : waveSpeed;
        float currentDamage = isPhase2 ? slamDamage * 1.2f : slamDamage;

        wave.Initialize(spawnPos, direction, currentWaveSpeed, currentDamage, waveSprite);
    }

    // -------------------------------------------------------------------------
    // Phase Transition
    // -------------------------------------------------------------------------

    private void CheckPhaseTransition(float current, float max)
    {
        if (!isPhase2 && (current / max) <= phase2HealthThreshold)
        {
            isPhase2 = true;
            StartCoroutine(Phase2Transition());
        }
    }

    private IEnumerator Phase2Transition()
    {
        currentState = BossState.Stunned;
        isAttacking = true;

        for (int i = 0; i < 6; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = i % 2 == 0 ? Color.yellow : Color.white;
            yield return new WaitForSeconds(0.15f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        if (rockSpawner != null)
            rockSpawner.SetDifficulty(1.3f, 0.6f);

        isAttacking = false;
        currentState = BossState.Chase;
    }

    // -------------------------------------------------------------------------
    // Death
    // -------------------------------------------------------------------------

    private void HandleDeath()
    {
        isDead = true;
        currentState = BossState.Dead;

        if (rockSpawner != null)
            rockSpawner.StopSpawning();

        // Grant the player the ground shockwave ability
        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            PlayerGroundShockwave shockwave = playerObject.GetComponent<PlayerGroundShockwave>();
            if (shockwave != null)
                shockwave.UnlockAbility();
            else
                Debug.LogWarning("EarthBossAI: PlayerGroundShockwave not found on player!");
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        PlayAnim("Death");

        for (int i = 0; i < 10; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = i % 2 == 0 ? Color.red : Color.white;
            yield return new WaitForSeconds(0.15f);
        }

        float fadeTime = 1f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f - (elapsed / fadeTime));
            yield return null;
        }

        // ✅ Don't destroy here — let EarthBossDeathHandler handle it
        // Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}