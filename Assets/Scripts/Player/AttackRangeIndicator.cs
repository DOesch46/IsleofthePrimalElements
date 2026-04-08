using UnityEngine;

/// <summary>
/// Shows a semi-transparent circle around the player indicating melee attack range.
/// Creates itself automatically — no manual setup needed in the scene.
/// </summary>
public class AttackRangeIndicator : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color rangeColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private int sortingOrder = -1;

    private GameObject indicator;
    private SpriteRenderer sr;
    private float currentRange;

    private void Start()
    {
        CreateIndicator();

        // Sync with PlayerCombat's attack range
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
            SetRange(combat.GetAttackRange());
    }

    public void SetRange(float range)
    {
        currentRange = range;
        if (indicator != null)
        {
            // Scale the circle to match the diameter (range on each side)
            float diameter = range * 2f;
            indicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    private void CreateIndicator()
    {
        // Try to load the existing Circle sprite from the project
        Sprite circleSprite = Resources.Load<Sprite>("Circle");

        indicator = new GameObject("AttackRangeIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = Vector3.zero;

        sr = indicator.AddComponent<SpriteRenderer>();
        sr.color = rangeColor;
        sr.sortingOrder = sortingOrder;

        if (circleSprite != null)
        {
            sr.sprite = circleSprite;
        }
        else
        {
            // Fallback: create a simple circle texture at runtime
            sr.sprite = CreateCircleSprite();
        }

        SetRange(currentRange);
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
