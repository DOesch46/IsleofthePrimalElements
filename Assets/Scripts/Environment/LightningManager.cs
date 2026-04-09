using UnityEngine;

public class LightningManager : MonoBehaviour
{
    public Transform[] nodes;
    public GameObject lightningPrefab;
    public float interval = 2f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnLightning), 1f, interval);
    }

    void SpawnLightning()
    {
        if (nodes.Length < 2) return;

        int a = Random.Range(0, nodes.Length);
        int b = Random.Range(0, nodes.Length);

        if (a == b) return;

        Transform nodeA = nodes[a];
        Transform nodeB = nodes[b];

        GameObject beam = Instantiate(lightningPrefab);
        beam.GetComponent<LightningBeam>().Initialize(nodeA, nodeB);
    }
}