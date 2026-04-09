using UnityEngine;
using System.Collections;

public class Knockback : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 sourcePosition, float force)
    {
        if (rb == null) return;
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        StartCoroutine(DoKnockback(direction, force));
    }

    private IEnumerator DoKnockback(Vector2 direction, float force)
    {
        rb.linearVelocity = direction * force;
        yield return new WaitForSeconds(0.15f);
        rb.linearVelocity = Vector2.zero;
    }
}