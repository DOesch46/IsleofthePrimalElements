using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player == null) return;

        Debug.Log("Lightning HIT for: " + damage);

        player.TakeDamage(damage);
    }
}