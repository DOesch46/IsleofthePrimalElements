using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPoint : MonoBehaviour
{
    [FormerlySerializedAs("transitionId")]
    [SerializeField] private string spawnId;

    [Header("Spawn Point")]
    [SerializeField] private bool isDefault = false;

    public string SpawnId => spawnId;
    public string TransitionId => spawnId;
    public bool IsDefault => isDefault;

    public bool Matches(string requestedSpawnId)
    {
        return !string.IsNullOrWhiteSpace(requestedSpawnId) &&
               string.Equals(spawnId, requestedSpawnId, System.StringComparison.Ordinal);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isDefault ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
