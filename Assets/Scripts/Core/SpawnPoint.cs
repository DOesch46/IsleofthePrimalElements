using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string transitionId;
    [SerializeField] private bool isDefault = false;

    public string TransitionId => transitionId;
    public bool IsDefault => isDefault;

    private void OnDrawGizmos()
    {
        Gizmos.color = isDefault ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}