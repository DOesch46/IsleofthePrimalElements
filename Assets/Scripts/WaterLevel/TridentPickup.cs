using UnityEngine;

public class TridentPickup : MonoBehaviour
{
    [SerializeField] private ElementType unlockElement = ElementType.Water;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject playerObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.transform.root.gameObject;

        if (!playerObject.CompareTag("Player"))
            return;

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CollectElement(unlockElement);
        }
        else
        {
            Debug.LogWarning("GameProgressManager.Instance is NULL");
        }

        Destroy(gameObject);
    }
}
