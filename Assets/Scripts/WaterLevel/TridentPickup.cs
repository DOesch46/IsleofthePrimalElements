using UnityEngine;

public class TridentPickup : MonoBehaviour
{
    [SerializeField] private ElementType unlockElement = ElementType.Water;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameProgressManager.Instance != null)
        {
            Debug.Log("Trident picked up, unlocking " + unlockElement);
            GameProgressManager.Instance.CollectElement(unlockElement);
        }
        else
        {
            Debug.LogWarning("GameProgressManager.Instance is null");
        }

        Destroy(gameObject);
    }
}