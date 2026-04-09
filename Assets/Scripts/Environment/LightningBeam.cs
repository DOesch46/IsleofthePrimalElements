using UnityEngine;

public class LightningBeam : MonoBehaviour
{
    public float duration = 1f;
    public float damagePerSecond = 20f;

    private Transform start;
    private Transform end;
    private BoxCollider2D col;

    public void Initialize(Transform a, Transform b)
    {
        start = a;
        end = b;

        col = GetComponent<BoxCollider2D>();

        UpdatePosition();

        Destroy(gameObject, duration);
    }

    void UpdatePosition()
{
    Vector3 dir = end.position - start.position;
    float length = dir.magnitude - 0.5f;

    // center between nodes
    transform.position = (start.position + end.position) / 2f;

    // rotate toward end
    transform.right = dir.normalized;

    // THIS is the key fix
    transform.localScale = new Vector3(length, 1.5f, 1f);}

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}