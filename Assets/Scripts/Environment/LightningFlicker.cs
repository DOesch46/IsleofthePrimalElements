using System.Collections;
using UnityEngine;

public class LightningStrikeWithWarning : MonoBehaviour
{
    public float minDelay = 2f;
    public float maxDelay = 6f;

    public float warningDuration = 0.5f;
    public float strikeDuration = 0.2f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D hitbox;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();

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

            yield return new WaitForSeconds(strikeDuration);

            // TURN OFF
            hitbox.enabled = false;
            spriteRenderer.enabled = false;
        }
    }
}