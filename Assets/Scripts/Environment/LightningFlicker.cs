using System.Collections;
using UnityEngine;

public class LightningStrikeWithWarning : MonoBehaviour
{
    public float minDelay = 2f;
    public float maxDelay = 6f;

    public float warningDuration = 0.5f;
    public float strikeDuration = 0.2f;

    [Header("Damage")]
    [SerializeField] private float strikeDamage = 20f;

    [Header("Sound")]
    [SerializeField] private AudioClip strikeSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.8f;
    [SerializeField] private float hearingRange = 10f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D hitbox;
    private bool isStriking = false;
    private Transform player;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        spriteRenderer.enabled = false;
        hitbox.enabled = false;

        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            // Wait randomly
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // WARNING PHASE (faint flash)
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f); // semi-transparent

            yield return new WaitForSeconds(warningDuration);

            // STRIKE PHASE (full lightning)
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // full brightness
            animator.Play("LightningFlash", 0, 0f);

            hitbox.enabled = true;
            isStriking = true;

            // Play strike sound only if player is close enough
            PlayStrikeSound();

            yield return new WaitForSeconds(strikeDuration);

            // TURN OFF
            isStriking = false;
            hitbox.enabled = false;
            spriteRenderer.enabled = false;
        }
    }

    // -------------------------------------------------------------------------
    // Damage — only during the strike phase
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player ENTERS the hitbox during a strike.
    /// Handles the case where the player walks into an active bolt.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isStriking) return;
        if (!other.CompareTag("Player")) return;

        DealDamage(other);
    }

    /// <summary>
    /// Called when the hitbox enables while the player is already inside.
    /// Handles the case where the player is standing still and gets struck.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isStriking) return;
        if (!other.CompareTag("Player")) return;

        DealDamage(other);
    }

    private void PlayStrikeSound()
    {
        if (strikeSound == null || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > hearingRange) return;

        // Volume fades with distance — louder when closer
        float volumeScale = 1f - (dist / hearingRange);
        AudioSource.PlayClipAtPoint(strikeSound, transform.position, soundVolume * volumeScale);
    }

    private void DealDamage(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(strikeDamage);
            Debug.Log($"LIGHTNING HIT! Dealt {strikeDamage} damage. Player health: {health.GetCurrentHealth()}/{health.GetMaxHealth()}");
        }
    }

    // -------------------------------------------------------------------------
    // Editor Gizmo — shows the damage area in Scene view
    // -------------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        // Yellow when idle, Red when striking
        Gizmos.color = isStriking
            ? new Color(1f, 0f, 0f, 0.5f)
            : new Color(1f, 1f, 0f, 0.25f);

        if (col is BoxCollider2D box)
        {
            Vector3 center = transform.position + (Vector3)box.offset;
            Vector3 size = box.size;
            Gizmos.DrawWireCube(center, size);
        }
        else if (col is CircleCollider2D circle)
        {
            Vector3 center = transform.position + (Vector3)circle.offset;
            Gizmos.DrawWireSphere(center, circle.radius);
        }
    }
}