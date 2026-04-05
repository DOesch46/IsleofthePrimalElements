using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player's equipped abilities.
/// Attach this to the Player GameObject.
/// 
/// Controls:
/// - Left Click or J: Use Ability 1
/// - Right Click or K: Use Ability 2
/// - Q/E: Switch between ability sets (if player has multiple)
/// 
/// How your team uses this:
/// 1. Create AbilityData assets for your level's abilities
/// 2. Call AbilityManager.Instance.EquipAbility() to give player an ability
/// 3. The system handles cooldowns, input, and activation
/// </summary>
public class AbilityManager : MonoBehaviour
{
    // =====================================================
    // SINGLETON
    // =====================================================

    private static AbilityManager instance;
    public static AbilityManager Instance => instance;

    // =====================================================
    // INSPECTOR SETTINGS
    // =====================================================

    [Header("Equipped Abilities")]
    [Tooltip("Ability in slot 1 (Left Click / J)")]
    [SerializeField] private AbilityData abilitySlot1;

    [Tooltip("Ability in slot 2 (Right Click / K)")]
    [SerializeField] private AbilityData abilitySlot2;

    [Header("All Unlocked Abilities")]
    [Tooltip("All abilities the player has unlocked (for reference)")]
    [SerializeField] private List<AbilityData> unlockedAbilities = new List<AbilityData>();

    [Header("Settings")]
    [Tooltip("Where projectiles spawn from (drag a child transform here)")]
    [SerializeField] private Transform abilitySpawnPoint;

    [Tooltip("Which direction the player is facing (1 = right, -1 = left)")]
    [SerializeField] private float facingDirection = 1f;

    [Header("Debug Info")]
    [SerializeField] private float slot1CooldownRemaining = 0f;
    [SerializeField] private float slot2CooldownRemaining = 0f;

    // =====================================================
    // PRIVATE DATA
    // =====================================================

    // Cooldown timers
    private float slot1NextUseTime = 0f;
    private float slot2NextUseTime = 0f;

    // Reference to sprite renderer for facing direction
    private SpriteRenderer spriteRenderer;
    
    // Stores the current aim direction (up, down, left, right)
    private Vector2 aimDirection = Vector2.right;

    // =====================================================
    // PROPERTIES
    // =====================================================

    /// <summary>
    /// Get the ability in slot 1.
    /// </summary>
    public AbilityData Slot1Ability => abilitySlot1;

    /// <summary>
    /// Get the ability in slot 2.
    /// </summary>
    public AbilityData Slot2Ability => abilitySlot2;

    /// <summary>
    /// Get all unlocked abilities.
    /// </summary>
    public List<AbilityData> UnlockedAbilities => unlockedAbilities;

    /// <summary>
    /// Check if slot 1 ability is ready to use.
    /// </summary>
    public bool IsSlot1Ready => Time.time >= slot1NextUseTime;

    /// <summary>
    /// Check if slot 2 ability is ready to use.
    /// </summary>
    public bool IsSlot2Ready => Time.time >= slot2NextUseTime;

    // =====================================================
    // UNITY LIFECYCLE
    // =====================================================

    private void Awake()
    {
        // Set singleton (not DontDestroyOnLoad since it's on the player)
        instance = this;

        spriteRenderer = GetComponent<SpriteRenderer>();

        // If no spawn point assigned, create one
        if (abilitySpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("AbilitySpawnPoint");
            spawnObj.transform.parent = transform;
            spawnObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            abilitySpawnPoint = spawnObj.transform;
        }
    }

    private void Update()
    {
        // Update facing direction based on sprite flip
        UpdateFacingDirection();

        // Handle input
        HandleAbilityInput();

        // Update debug cooldown display
        UpdateDebugInfo();
    }

    // =====================================================
    // INPUT HANDLING
    // =====================================================

    /// <summary>
    /// Reads input and activates abilities.
    /// </summary>
    private void HandleAbilityInput()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null) return;

        // Ability 1: Left Click or J
        if ((mouse != null && mouse.leftButton.wasPressedThisFrame) ||
            keyboard.jKey.wasPressedThisFrame)
        {
            UseAbility(1);
        }

        // Ability 2: Right Click or K
        if ((mouse != null && mouse.rightButton.wasPressedThisFrame) ||
            keyboard.kKey.wasPressedThisFrame)
        {
            UseAbility(2);
        }
    }

    /// <summary>
    /// Updates which direction the player is facing and aiming.
    /// Supports all 4 directions based on last key pressed.
    /// </summary>
    private void UpdateFacingDirection()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Update aim direction based on input
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            aimDirection = Vector2.left;
            facingDirection = -1f;
        }
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            aimDirection = Vector2.right;
            facingDirection = 1f;
        }
        else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            aimDirection = Vector2.up;
        }
        else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            aimDirection = Vector2.down;
        }

        // Update spawn point position based on aim
        if (abilitySpawnPoint != null)
        {
            abilitySpawnPoint.localPosition = new Vector3(
                aimDirection.x * 1f,
                aimDirection.y * 1f,
                0f
            );
        }
    }

    // =====================================================
    // ABILITY USAGE
    // =====================================================

    /// <summary>
    /// Uses the ability in the specified slot.
    /// </summary>
    /// <param name="slot">1 or 2</param>
    public void UseAbility(int slot)
    {
        AbilityData ability = null;
        bool isReady = false;

        if (slot == 1)
        {
            ability = abilitySlot1;
            isReady = IsSlot1Ready;
        }
        else if (slot == 2)
        {
            ability = abilitySlot2;
            isReady = IsSlot2Ready;
        }

        // Check if ability exists
        if (ability == null)
        {
            Debug.Log($"No ability equipped in slot {slot}!");
            return;
        }

        // Check cooldown
        if (!isReady)
        {
            float remaining = slot == 1 ? slot1CooldownRemaining : slot2CooldownRemaining;
            Debug.Log($"{ability.abilityName} is on cooldown! {remaining:F1}s remaining.");
            return;
        }

        // Activate the ability
        ActivateAbility(ability, slot);
    }

    /// <summary>
    /// Activates an ability based on its type.
    /// This is where the magic happens!
    /// </summary>
    private void ActivateAbility(AbilityData ability, int slot)
    {
        Debug.Log($"Using ability: {ability.abilityName} (Slot {slot})");

        // Start cooldown
        if (slot == 1)
        {
            slot1NextUseTime = Time.time + ability.cooldown;
        }
        else
        {
            slot2NextUseTime = Time.time + ability.cooldown;
        }

        // Activate based on type
        switch (ability.abilityType)
        {
            case AbilityType.Projectile:
                SpawnProjectile(ability);
                break;

            case AbilityType.AreaEffect:
                SpawnAreaEffect(ability);
                break;

            case AbilityType.Buff:
                ApplyBuff(ability);
                break;

            case AbilityType.Melee:
                PerformMelee(ability);
                break;

            default:
                Debug.LogWarning($"Unknown ability type: {ability.abilityType}");
                break;
        }

        // Spawn cast visual effect
        if (ability.castEffectPrefab != null)
        {
            Instantiate(ability.castEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    // =====================================================
    // ABILITY TYPE IMPLEMENTATIONS
    // =====================================================

    /// <summary>
    /// Spawns a projectile that flies in the aim direction.
    /// </summary>
    private void SpawnProjectile(AbilityData ability)
    {
        if (ability.projectilePrefab == null)
        {
            Debug.LogWarning($"{ability.abilityName}: No projectile prefab assigned!");
            SpawnDefaultProjectile(ability);
            return;
        }

        // Spawn the projectile
        Vector3 spawnPos = abilitySpawnPoint != null ? abilitySpawnPoint.position : transform.position;
        GameObject projectile = Instantiate(ability.projectilePrefab, spawnPos, Quaternion.identity);

        // Set up the projectile with aim direction
        AbilityProjectile projScript = projectile.GetComponent<AbilityProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(ability, aimDirection);
        }

        Debug.Log($"Spawned projectile: {ability.abilityName} → {aimDirection}");
    }

    /// <summary>
    /// Spawns a default projectile when no prefab is assigned.
    /// </summary>
    private void SpawnDefaultProjectile(AbilityData ability)
    {
        Vector3 spawnPos = abilitySpawnPoint != null ? abilitySpawnPoint.position : transform.position;

        // Create a simple projectile
        GameObject projectile = new GameObject($"{ability.abilityName}_Projectile");
        projectile.transform.position = spawnPos;

        // Add visual
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = ability.icon;
        sr.color = ability.abilityColor != Color.white ? ability.abilityColor : ability.GetElementColor();
        projectile.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        // Add physics
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = aimDirection * ability.projectileSpeed;

        // Add collider
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        // Add projectile script
        AbilityProjectile projScript = projectile.AddComponent<AbilityProjectile>();
        projScript.Initialize(ability, aimDirection);

        // Set sorting layer
        sr.sortingLayerName = "Decor";
        sr.sortingOrder = 10;

        Debug.Log($"Spawned default projectile: {ability.abilityName} → {aimDirection}");
    }

    /// <summary>
    /// Spawns an area effect around the player or at a target position.
    /// </summary>
    private void SpawnAreaEffect(AbilityData ability)
    {
        Vector3 effectPos = transform.position;

        if (ability.areaEffectPrefab != null)
        {
            GameObject effect = Instantiate(ability.areaEffectPrefab, effectPos, Quaternion.identity);
            
            // Set up the area effect
            AbilityAreaEffect areaScript = effect.GetComponent<AbilityAreaEffect>();
            if (areaScript != null)
            {
                areaScript.Initialize(ability);
            }

            Debug.Log($"Spawned area effect: {ability.abilityName}");
        }
        else
        {
            // Default area effect - damage everything in radius
            Debug.Log($"Area effect: {ability.abilityName} - Radius: {ability.areaRadius}");
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(effectPos, ability.areaRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Debug.Log($"Area effect hit: {hit.gameObject.name}");
                    // Deal damage to enemy - we'll connect this later
                }
            }
        }
    }

    /// <summary>
    /// Applies a temporary buff to the player.
    /// </summary>
    private void ApplyBuff(AbilityData ability)
    {
        Debug.Log($"Buff applied: {ability.abilityName} - {ability.buffStat} +{ability.buffAmount} for {ability.duration}s");
        
        // Start the buff coroutine
        StartCoroutine(BuffRoutine(ability));
    }

    /// <summary>
    /// Coroutine that handles buff duration.
    /// </summary>
    private System.Collections.IEnumerator BuffRoutine(AbilityData ability)
    {
        // Apply buff
        ApplyBuffEffect(ability, true);

        // Wait for duration
        yield return new WaitForSeconds(ability.duration);

        // Remove buff
        ApplyBuffEffect(ability, false);
        Debug.Log($"Buff expired: {ability.abilityName}");
    }

    /// <summary>
    /// Applies or removes a buff effect.
    /// Your team can expand this with more buff types.
    /// </summary>
    private void ApplyBuffEffect(AbilityData ability, bool apply)
    {
        float amount = apply ? ability.buffAmount : -ability.buffAmount;

        switch (ability.buffStat)
        {
            case BuffStat.Speed:
                // Modify player speed - connect to your movement system
                Debug.Log($"Speed {(apply ? "increased" : "decreased")} by {ability.buffAmount}");
                break;

            case BuffStat.Defense:
                Debug.Log($"Defense {(apply ? "increased" : "decreased")} by {ability.buffAmount}");
                break;

            case BuffStat.AttackDamage:
                Debug.Log($"Attack {(apply ? "increased" : "decreased")} by {ability.buffAmount}");
                break;

            case BuffStat.Health:
                if (apply)
                {
                    Debug.Log($"Healed for {ability.buffAmount}");
                }
                break;

            default:
                Debug.Log($"Buff stat {ability.buffStat} not implemented yet.");
                break;
        }
    }

    /// <summary>
    /// Performs a melee attack in the aim direction.
    /// </summary>
    private void PerformMelee(AbilityData ability)
    {
        Vector2 attackPos = (Vector2)transform.position + aimDirection * (ability.range * 0.5f);

        Debug.Log($"Melee attack: {ability.abilityName} → {aimDirection}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, ability.range);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"Melee hit: {hit.gameObject.name}");
                hit.gameObject.SendMessage("TakeDamage", ability.damage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // =====================================================
    // EQUIP / UNEQUIP
    // =====================================================

    /// <summary>
    /// Equips an ability to a slot.
    /// Call this when player picks an ability after boss defeat.
    /// 
    /// Example: AbilityManager.Instance.EquipAbility(fireballData, 1);
    /// </summary>
    public void EquipAbility(AbilityData ability, int slot)
    {
        if (ability == null) return;

        if (slot == 1)
        {
            abilitySlot1 = ability;
            Debug.Log($"Equipped {ability.abilityName} to Slot 1");
        }
        else if (slot == 2)
        {
            abilitySlot2 = ability;
            Debug.Log($"Equipped {ability.abilityName} to Slot 2");
        }

        // Add to unlocked list if not already there
        if (!unlockedAbilities.Contains(ability))
        {
            unlockedAbilities.Add(ability);
        }
    }

    /// <summary>
    /// Unlocks an ability without equipping it.
    /// </summary>
    public void UnlockAbility(AbilityData ability)
    {
        if (ability == null) return;

        if (!unlockedAbilities.Contains(ability))
        {
            unlockedAbilities.Add(ability);
            Debug.Log($"Ability unlocked: {ability.abilityName}");
        }
    }

    /// <summary>
    /// Removes ability from a slot.
    /// </summary>
    public void UnequipAbility(int slot)
    {
        if (slot == 1)
        {
            Debug.Log($"Unequipped {abilitySlot1?.abilityName} from Slot 1");
            abilitySlot1 = null;
        }
        else if (slot == 2)
        {
            Debug.Log($"Unequipped {abilitySlot2?.abilityName} from Slot 2");
            abilitySlot2 = null;
        }
    }

    // =====================================================
    // COOLDOWN HELPERS
    // =====================================================

    /// <summary>
    /// Gets the remaining cooldown for a slot (0 = ready).
    /// </summary>
    public float GetCooldownRemaining(int slot)
    {
        if (slot == 1)
        {
            return Mathf.Max(0f, slot1NextUseTime - Time.time);
        }
        else
        {
            return Mathf.Max(0f, slot2NextUseTime - Time.time);
        }
    }

    /// <summary>
    /// Gets cooldown progress for a slot (0 = on cooldown, 1 = ready).
    /// Useful for UI cooldown indicators.
    /// </summary>
    public float GetCooldownProgress(int slot)
    {
        AbilityData ability = slot == 1 ? abilitySlot1 : abilitySlot2;
        if (ability == null || ability.cooldown <= 0) return 1f;

        float remaining = GetCooldownRemaining(slot);
        return 1f - (remaining / ability.cooldown);
    }

    // =====================================================
    // DEBUG
    // =====================================================

    private void UpdateDebugInfo()
    {
        slot1CooldownRemaining = GetCooldownRemaining(1);
        slot2CooldownRemaining = GetCooldownRemaining(2);
    }

    [ContextMenu("Debug: List Abilities")]
    public void DebugListAbilities()
    {
        Debug.Log($"Slot 1: {(abilitySlot1 != null ? abilitySlot1.abilityName : "Empty")}");
        Debug.Log($"Slot 2: {(abilitySlot2 != null ? abilitySlot2.abilityName : "Empty")}");
        Debug.Log($"Unlocked: {unlockedAbilities.Count} abilities");
    }
}