using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    public BossController.BossState zoneState;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BossController boss = other.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.SetState(zoneState);
        }
    }
}