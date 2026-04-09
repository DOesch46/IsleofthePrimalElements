using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class EarthCollapse : MonoBehaviour
{
    private TilemapCollider2D tilemapCollider;
    public Tilemap tilemap;
    public float collapseInterval = 2f;

    // ✅ NEW — Earth area only
    public Vector3Int minBounds;
    public Vector3Int maxBounds;

    private BoundsInt bounds;
    private Coroutine collapseRoutine;

    public void StartCollapse()
    {
        if (collapseRoutine == null)
        {
            bounds = new BoundsInt(minBounds, maxBounds - minBounds);
            collapseRoutine = StartCoroutine(CollapseRoutine());
        }
    }

    public void StopCollapse()
    {
        if (collapseRoutine != null)
        {
            StopCoroutine(collapseRoutine);
            collapseRoutine = null;
        }
    }

    IEnumerator CollapseRoutine()
    {
        while (bounds.size.x > 0 && bounds.size.y > 0)
        {
            CollapseOuterLayer();
            yield return new WaitForSeconds(collapseInterval);

            bounds.xMin++;
            bounds.yMin++;
        }
    }

   void CollapseOuterLayer()
{
    // Collapse LEFT edge
   for (int y = bounds.yMin; y < bounds.yMax; y++)
{
    if (Random.value > 0.2f) // 80% chance to remove
    {
        tilemap.SetTile(new Vector3Int(bounds.xMin, y, 0), null);
    }
}

    // Collapse BOTTOM edge
    for (int x = bounds.xMin; x < bounds.xMax; x++)
{
    if (Random.value > 0.2f) // 80% chance to remove
        tilemap.SetTile(new Vector3Int(x, bounds.yMin, 0), null);
}

    // 🔥 THIS IS THE IMPORTANT PART
    tilemapCollider.ProcessTilemapChanges();
}
void Start()
{
    tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
}
}