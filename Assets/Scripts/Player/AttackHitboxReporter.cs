using UnityEngine;

/// <summary>
/// Attach this to the child "AttackHitbox" GameObject under the Player.
/// It relays OnTriggerEnter2D events back up to PlayerCombat on the parent.
///
/// SETUP:
/// 1. Create a child GameObject of the Player named "AttackHitbox".
/// 2. Add a CircleCollider2D (Is Trigger = true).
/// 3. Add a Rigidbody2D (Kinematic, Gravity Scale = 0) — needed for 2D trigger detection.
/// 4. Attach this script.
/// 5. Drag this child GameObject into PlayerCombat's attackHitbox field.
/// 6. Disable the GameObject in the Hierarchy — PlayerCombat will enable/disable it.
/// </summary>
public class AttackHitboxReporter : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private void Awake()
    {
        // Walk up to the parent to find PlayerCombat
        playerCombat = GetComponentInParent<PlayerCombat>();

        if (playerCombat == null)
            Debug.LogWarning("AttackHitboxReporter: No PlayerCombat found on parent. Assign correctly.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player's own colliders
        if (other.transform.IsChildOf(transform.parent)) return;

        playerCombat?.OnHitboxTrigger(other);
    }
}
