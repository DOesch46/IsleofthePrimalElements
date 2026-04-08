using UnityEngine;

public class PlayerWaterAbility : MonoBehaviour
{
    [Header("Input")]
    public KeyCode abilityKey = KeyCode.Q;

    [Header("Charge")]
    public float maxChargeTime = 1.5f;
    private float chargeTime = 0f;
    private bool isCharging = false;

    [Header("Projectile")]
    public GameObject wavePrefab;

    [Header("Wave Sprites")]
    public Sprite smallWaveSprite;
    public Sprite mediumWaveSprite;
    public Sprite largeWaveSprite;

    [Header("Damage")]
    public float smallDamage = 10f;
    public float mediumDamage = 20f;
    public float largeDamage = 30f;

    [Header("Speed")]
    public float smallSpeed = 6f;
    public float mediumSpeed = 9f;
    public float largeSpeed = 12f;

    [Header("Scale")]
    public Vector3 smallScale = new Vector3(0.35f, 0.35f, 1f);
    public Vector3 mediumScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 largeScale = new Vector3(0.7f, 0.7f, 1f);

    [Header("Spawn Offset")]
    public float spawnDistance = 0.6f;

    private bool hasAbility = false;
    private Vector2 lastDirection = Vector2.right;

    void Start()
    {
        Debug.Log("PlayerWaterAbility running on " + gameObject.name);
    }

    void Update()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.Log("No GameProgressManager found");
            return;
        }

        hasAbility = GameProgressManager.Instance.HasElement(ElementType.Water);

        if (!hasAbility)
        {
            if (Input.GetKeyDown(abilityKey))
            {
                Debug.Log("Q pressed, but Water ability is not unlocked");
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.W)) lastDirection = Vector2.up;
        if (Input.GetKeyDown(KeyCode.S)) lastDirection = Vector2.down;
        if (Input.GetKeyDown(KeyCode.A)) lastDirection = Vector2.left;
        if (Input.GetKeyDown(KeyCode.D)) lastDirection = Vector2.right;

        if (Input.GetKeyDown(abilityKey))
        {
            Debug.Log("Started charging");
            isCharging = true;
            chargeTime = 0f;
        }

        if (isCharging && Input.GetKey(abilityKey))
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
        }

        if (isCharging && Input.GetKeyUp(abilityKey))
        {
            Debug.Log("Released charge");
            FireWave();
            isCharging = false;
            chargeTime = 0f;
        }
    }

    void FireWave()
    {
        Debug.Log("FireWave called");

        if (wavePrefab == null)
        {
            Debug.LogWarning("Wave prefab is missing");
            return;
        }

        float chargePercent = chargeTime / maxChargeTime;

        Sprite selectedSprite;
        float selectedDamage;
        float selectedSpeed;
        Vector3 selectedScale;

        if (chargePercent < 0.33f)
        {
            selectedSprite = smallWaveSprite;
            selectedDamage = smallDamage;
            selectedSpeed = smallSpeed;
            selectedScale = smallScale;
        }
        else if (chargePercent < 0.66f)
        {
            selectedSprite = mediumWaveSprite;
            selectedDamage = mediumDamage;
            selectedSpeed = mediumSpeed;
            selectedScale = mediumScale;
        }
        else
        {
            selectedSprite = largeWaveSprite;
            selectedDamage = largeDamage;
            selectedSpeed = largeSpeed;
            selectedScale = largeScale;
        }

        Vector3 spawnPos = transform.position + (Vector3)(lastDirection.normalized * spawnDistance);

        GameObject wave = Instantiate(wavePrefab, spawnPos, Quaternion.identity);
        wave.transform.localScale = selectedScale;

        ChargedWaveProjectile projectile = wave.GetComponent<ChargedWaveProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(selectedDamage, selectedSpeed, selectedSprite, lastDirection);
        }
        else
        {
            Debug.LogWarning("ChargedWaveProjectile component missing on wave prefab");
        }
    }
}