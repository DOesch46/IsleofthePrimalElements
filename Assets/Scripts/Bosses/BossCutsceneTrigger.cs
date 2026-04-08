using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Place this on an empty GameObject with a BoxCollider2D (isTrigger = true)
/// at the spot where the boss cutscene should begin.
///
/// SETUP IN UNITY:
/// 1. Create an empty GameObject in the LightningLevel scene (e.g. "BossCutsceneTrigger").
/// 2. Add a BoxCollider2D, check "Is Trigger", size it to cover the trigger area.
/// 3. Attach this script.
/// 4. Assign:
///    - bossGameObject   : The boss (e.g. Voltaris) — should start INACTIVE in the scene.
///    - dialoguePanel    : A UI panel with speaker name + body text (same setup as NPCDialogue).
///    - speakerNameText  : TextMeshProUGUI for the boss name.
///    - dialogueBodyText : TextMeshProUGUI for the dialogue line.
///    - dialogueLines[]  : The boss's pre-fight dialogue lines.
/// 5. Set autoWalkDirection and autoWalkDistance for the player's auto-walk.
///
/// FLOW:
///   Player enters trigger → input disabled → player auto-walks left →
///   boss dialogue plays (press E/interact to advance) → boss activates → input restored.
/// </summary>
public class BossCutsceneTrigger : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector — Auto-Walk Settings
    // -------------------------------------------------------------------------

    [Header("Auto-Walk")]
    [Tooltip("Direction the player walks during the cutscene intro.")]
    [SerializeField] private Vector2 autoWalkDirection = Vector2.left;

    [Tooltip("How far (in units) the player auto-walks.")]
    [SerializeField] private float autoWalkDistance = 3f;

    [Tooltip("Speed of the auto-walk (uses player's normal speed if 0).")]
    [SerializeField] private float autoWalkSpeed = 3f;

    // -------------------------------------------------------------------------
    // Inspector — Dialogue
    // -------------------------------------------------------------------------

    [Header("Boss Info")]
    [SerializeField] private string bossName = "Voltaris";

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines = new string[]
    {
        "So... you've made it this far.",
        "I am Voltaris, Herald of the Storm.",
        "You will not leave this place alive.",
        "Prepare yourself!"
    };

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;

    // -------------------------------------------------------------------------
    // Inspector — Boss Reference
    // -------------------------------------------------------------------------

    [Header("Boss")]
    [Tooltip("The boss GameObject. Leave it ACTIVE in the scene — the script will freeze it during cutscene and unfreeze after dialogue.")]
    [SerializeField] private GameObject bossGameObject;

    [Header("Level Music")]
    [Tooltip("The AudioSource playing background music. Gets stopped when the cutscene starts.")]
    [SerializeField] private AudioSource levelMusicSource;

    [Header("Boss Music")]
    [Tooltip("The music clip that plays when the cutscene starts and loops until the boss dies.")]
    [SerializeField] private AudioClip bossMusicClip;

    [Tooltip("Volume of the boss music (0 to 1).")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Arena Barrier")]
    [Tooltip("Width of the invisible wall that blocks the entrance (matches trigger width if 0).")]
    [SerializeField] private float barrierWidth = 0f;

    [Tooltip("Height of the invisible wall (how tall the barrier is).")]
    [SerializeField] private float barrierHeight = 5f;

    [Header("Exit Portal")]
    [Tooltip("The exit portal that appears when the boss dies. Should start INACTIVE.")]
    [SerializeField] private GameObject exitPortalObject;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private bool hasTriggered = false;
    private bool waitingForInput = false;
    private int currentLineIndex = 0;
    private PlayerController playerController;
    private PlayerInput playerInput;
    private Animator playerAnimator;
    private MonoBehaviour[] bossScripts;  // all boss scripts to freeze/unfreeze
    private AudioSource musicSource;      // created at runtime for boss music

    // -------------------------------------------------------------------------
    // Initialization — make boss invulnerable from the start
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (bossGameObject != null)
        {
            EnemyHealth bossHealth = bossGameObject.GetComponent<EnemyHealth>();
            if (bossHealth != null)
                bossHealth.SetInvulnerable(true);
        }
    }

    // -------------------------------------------------------------------------
    // Trigger Detection
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        // Cache player references
        playerController = other.GetComponent<PlayerController>();
        playerInput = other.GetComponent<PlayerInput>();
        playerAnimator = other.GetComponent<Animator>();

        StartCoroutine(PlayCutscene(other.transform));
    }

    // -------------------------------------------------------------------------
    // Cutscene Coroutine
    // -------------------------------------------------------------------------

    private IEnumerator PlayCutscene(Transform playerTransform)
    {
        // ----- PHASE 0: Kill all mini enemies & stop level music -----
        KillAllMiniEnemies();

        if (levelMusicSource != null)
            levelMusicSource.Stop();

        // ----- PHASE 1: Disable player input & freeze boss -----
        DisablePlayerInput();
        FreezeBoss();

        // Start boss music right away
        StartBossMusic();

        // Stop any current movement
        Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRB != null)
            playerRB.linearVelocity = Vector2.zero;

        // Small pause before auto-walk starts
        yield return new WaitForSeconds(0.3f);

        // ----- PHASE 2: Auto-walk the player into the arena -----
        yield return StartCoroutine(AutoWalkPlayer(playerTransform, playerRB));

        // Small pause after walk completes
        yield return new WaitForSeconds(0.4f);

        // ----- PHASE 3: Boss dialogue -----
        yield return StartCoroutine(PlayDialogue());

        // ----- PHASE 4: Seal the arena, unfreeze boss & restore control -----
        SpawnBarrier();
        UnfreezeBoss();
        EnablePlayerInput();

        // Start watching for boss death to stop the music
        // Attach a small watcher script to the music object so it survives this trigger being destroyed
        if (musicSource != null)
        {
            BossMusicWatcher watcher = musicSource.gameObject.AddComponent<BossMusicWatcher>();
            watcher.Setup(bossGameObject, musicSource);

            if (exitPortalObject != null)
                watcher.SetExitPortal(exitPortalObject);
        }

        // Destroy the trigger so it can't fire again
        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Auto-Walk
    // -------------------------------------------------------------------------

    private IEnumerator AutoWalkPlayer(Transform playerTransform, Rigidbody2D playerRB)
    {
        Vector2 walkDir = autoWalkDirection.normalized;
        float distanceTravelled = 0f;

        // Determine character prefix for walk animation
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 0);
        string prefix = selected == 0 ? "Blue" : "Red";

        // Play the correct walk animation based on direction
        if (playerAnimator != null)
        {
            if (walkDir.x < -0.1f)
                playerAnimator.Play(prefix + "WalkLeft", 0, 0f);
            else if (walkDir.x > 0.1f)
                playerAnimator.Play(prefix + "WalkRight", 0, 0f);
            else if (walkDir.y > 0.1f)
                playerAnimator.Play(prefix + "WalkUp", 0, 0f);
            else if (walkDir.y < -0.1f)
                playerAnimator.Play(prefix + "WalkDown", 0, 0f);
        }

        // Move the player via Rigidbody2D
        while (distanceTravelled < autoWalkDistance)
        {
            float step = autoWalkSpeed * Time.deltaTime;
            distanceTravelled += step;

            if (playerRB != null)
                playerRB.linearVelocity = walkDir * autoWalkSpeed;

            yield return null;
        }

        // Stop movement and play idle
        if (playerRB != null)
            playerRB.linearVelocity = Vector2.zero;

        if (playerAnimator != null)
            playerAnimator.Play(prefix + "Idle", 0, 0f);
    }

    // -------------------------------------------------------------------------
    // Dialogue System
    // -------------------------------------------------------------------------

    private IEnumerator PlayDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            yield break;

        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = bossName;

        currentLineIndex = 0;
        ShowLine(currentLineIndex);

        // Wait for the player to press interact (E) to advance each line
        while (currentLineIndex < dialogueLines.Length)
        {
            waitingForInput = true;

            // Wait until the player presses the interact key
            while (waitingForInput)
                yield return null;

            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
                ShowLine(currentLineIndex);
        }

        // Hide dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Brief pause after dialogue ends before fight begins
        yield return new WaitForSeconds(0.5f);
    }

    private void ShowLine(int index)
    {
        if (dialogueBodyText != null && index < dialogueLines.Length)
            dialogueBodyText.text = dialogueLines[index];
    }

    // -------------------------------------------------------------------------
    // Input Handling — listens for Interact even while PlayerInput is off
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (!waitingForInput) return;

        // Check for interact input (E key or gamepad south button)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            waitingForInput = false;
        }
        else if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            waitingForInput = false;
        }
    }

    // -------------------------------------------------------------------------
    // Boss Freeze / Unfreeze — boss stays visible but can't act
    // -------------------------------------------------------------------------

    /// <summary>
    /// Disables ALL scripts on the boss (EnemyAI, EnemyDamage, etc.)
    /// so it stays visible but doesn't move, attack, or deal damage.
    /// </summary>
    private void FreezeBoss()
    {
        if (bossGameObject == null) return;

        // Grab every MonoBehaviour on the boss and disable them all
        bossScripts = bossGameObject.GetComponents<MonoBehaviour>();

        foreach (var script in bossScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        // Also freeze the boss's Rigidbody so it doesn't drift
        Rigidbody2D bossRB = bossGameObject.GetComponent<Rigidbody2D>();
        if (bossRB != null)
        {
            bossRB.linearVelocity = Vector2.zero;
            bossRB.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    /// <summary>
    /// Re-enables all boss scripts so it starts fighting.
    /// Also removes invulnerability so the player can damage the boss.
    /// </summary>
    private void UnfreezeBoss()
    {
        if (bossScripts == null) return;

        foreach (var script in bossScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        // Remove invulnerability — boss can now take damage
        if (bossGameObject != null)
        {
            EnemyHealth bossHealth = bossGameObject.GetComponent<EnemyHealth>();
            if (bossHealth != null)
                bossHealth.SetInvulnerable(false);
        }
    }

    // -------------------------------------------------------------------------
    // Arena Barrier — blocks entrance after cutscene starts
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an invisible solid wall at the trigger position so
    /// the player can't leave and enemies can't enter.
    /// </summary>
    private void SpawnBarrier()
    {
        GameObject barrier = new GameObject("BossArenaBarrier");
        barrier.transform.position = transform.position;

        // Add a solid (non-trigger) collider — this blocks movement
        BoxCollider2D col = barrier.AddComponent<BoxCollider2D>();

        // Use trigger's own size if barrierWidth is 0
        BoxCollider2D triggerCol = GetComponent<BoxCollider2D>();
        float width = barrierWidth > 0f
            ? barrierWidth
            : (triggerCol != null ? triggerCol.size.x : 2f);

        col.size = new Vector2(width, barrierHeight);
        col.isTrigger = false; // solid wall!

        // Add a Rigidbody2D set to Static so it doesn't move
        Rigidbody2D rb = barrier.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Add the barrier script to destroy enemies that bump into it
        barrier.AddComponent<BossArenaBarrier>();
    }

    /// <summary>
    /// Destroys ALL mini enemies in the entire scene.
    /// Does NOT destroy the boss.
    /// </summary>
    private void KillAllMiniEnemies()
    {
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in allEnemies)
        {
            // Don't destroy the boss!
            if (bossGameObject != null && enemy.gameObject == bossGameObject)
                continue;

            Destroy(enemy.gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Boss Music
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an AudioSource and starts playing the boss music on loop.
    /// </summary>
    private void StartBossMusic()
    {
        if (bossMusicClip == null) return;

        // Create a separate GameObject so it survives this trigger being destroyed
        GameObject musicObj = new GameObject("BossMusic");
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.clip = bossMusicClip;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();
    }


    // -------------------------------------------------------------------------
    // Input Control
    // -------------------------------------------------------------------------

    private void DisablePlayerInput()
    {
        // Disable the PlayerInput component so the player can't move or attack
        if (playerInput != null)
            playerInput.enabled = false;

        // Also flag UI as open so PlayerController.Update() calls Stop()
        GameStateManager.IsUIOpen = true;
    }

    private void EnablePlayerInput()
    {
        // Re-enable PlayerInput
        if (playerInput != null)
            playerInput.enabled = true;

        GameStateManager.IsUIOpen = false;
    }
}
