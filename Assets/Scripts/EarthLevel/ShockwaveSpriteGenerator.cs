using UnityEngine;

/// <summary>
/// Attach to an empty GameObject, hit Play, and it will generate
/// a shockwave sprite and save the reference. Use this for your prefab.
/// </summary>
public class ShockwaveSpriteGenerator : MonoBehaviour
{
    [Header("Generated Sprite Settings")]
    [SerializeField] private int textureWidth = 64;
    [SerializeField] private int textureHeight = 32;
    [SerializeField] private Color coreColor = new Color(0.6f, 0.35f, 0.1f, 1f);    // brown
    [SerializeField] private Color edgeColor = new Color(0.85f, 0.65f, 0.3f, 0.8f);  // sandy gold
    [SerializeField] private Color outlineColor = new Color(0.3f, 0.15f, 0.05f, 1f); // dark brown

    public Sprite GeneratedSprite { get; private set; }

    private void Awake()
    {
        GeneratedSprite = CreateShockwaveSprite();
    }

    public Sprite CreateShockwaveSprite()
    {
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        float cx = textureWidth / 2f;
        float cy = textureHeight / 2f;
        float rx = textureWidth / 2f;
        float ry = textureHeight / 2f;

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > 1f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (dist > 0.85f)
                {
                    // Outer edge / outline
                    tex.SetPixel(x, y, outlineColor);
                }
                else if (dist > 0.5f)
                {
                    // Middle ring — gradient from edge to core
                    float t = (dist - 0.5f) / 0.35f;
                    Color c = Color.Lerp(coreColor, edgeColor, t);
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    // Inner core — bright center
                    float t = dist / 0.5f;
                    Color bright = new Color(1f, 0.9f, 0.6f, 1f); // bright yellow
                    Color c = Color.Lerp(bright, coreColor, t);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            32f
        );

        sprite.name = "GroundShockwaveSprite";
        return sprite;
    }
}