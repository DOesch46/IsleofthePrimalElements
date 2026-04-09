using UnityEngine;

public class OneWayDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D doorCollider;

    private bool playerInside = false;

    private void Start()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();

        doorCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerInside)
        {
            playerInside = false;
            // Don't lock the door anymore — keep it as trigger so player can re-enter
            Debug.Log("Player passed through arena door.");
        }
    }

    public void UnlockDoor()
    {
        doorCollider.isTrigger = true;
        playerInside = false;
        Debug.Log("Arena door unlocked!");
    }
}