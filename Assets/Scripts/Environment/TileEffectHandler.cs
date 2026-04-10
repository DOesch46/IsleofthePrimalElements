using UnityEngine;
using UnityEngine.Tilemaps;

public class TileEffectHandler : MonoBehaviour
{
    
    private float lavaTimer = 0f;
    public float lavaTickInterval = 0.5f;   // damage every 0.5s
    public float lavaDamagePerTick = 5f;    // adjust as you like
    public Tilemap tilemap;

    public float damagePerSecond = 10f;
    public float waterSlowMultiplier = 0.5f;

    private PlayerHealth playerHealth;
    private MovementSystem movementSystem;

    private float originalSpeed;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        movementSystem = GetComponent<MovementSystem>();

        originalSpeed = movementSystem.GetMoveSpeed();
    }

    void Update()
{
    Vector3 worldPos = transform.position;
    Vector3Int cellPos = tilemap.WorldToCell(worldPos);

    TileBase tile = tilemap.GetTile(cellPos);

    if (tile == null)
    {
        Debug.Log("No tile under player");
        return;
    }

    string tileName = tile.name.ToLower();
    //Debug.Log("Standing on: " + tileName);

   // 🔥 LAVA
if (tileName.Contains("lava"))
{
    lavaTimer += Time.deltaTime;

    if (lavaTimer >= lavaTickInterval)
    {
        lavaTimer = 1f;

        Debug.Log("🔥 Taking lava damage");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(lavaDamagePerTick);
        }
    }
}
else
{
    // reset when not on lava
    lavaTimer = 0f;
}

    // 🌊 WATER
    if (tileName.Contains("water"))
    {
        Debug.Log("ON WATER");
        movementSystem.SetMoveSpeed(originalSpeed * waterSlowMultiplier);
    }
    else
    {
        movementSystem.SetMoveSpeed(originalSpeed);
    }
}
}