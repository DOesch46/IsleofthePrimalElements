using UnityEngine;

public class AutoCollect : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ItemPickup pickup = GetComponent<ItemPickup>();
        if (pickup != null && pickup.IsInteractable)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pickup.Interact(pc);
        }
    }
}