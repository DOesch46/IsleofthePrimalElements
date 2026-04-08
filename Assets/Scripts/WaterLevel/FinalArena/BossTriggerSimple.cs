using UnityEngine;

public class BossTriggerSimple : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            
            if (bossObject != null)
                bossObject.SetActive(true);

            gameObject.SetActive(false); // disables trigger after use
        }
    }
}