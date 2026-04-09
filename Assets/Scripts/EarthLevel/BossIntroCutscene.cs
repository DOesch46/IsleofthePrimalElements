using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BossIntroCutscene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject earthBoss;
    [SerializeField] private Transform playerWalkTarget;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;

    [Header("Settings")]
    [SerializeField] private float playerWalkSpeed = 2f;
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Boss Fight References")]
    [SerializeField] private GameObject bossHealthBar;
    [SerializeField] private FallingRockSpawner rockSpawner;
    [SerializeField] private EarthPillarSpawner pillarSpawner;

    private bool cutsceneActive = false;
    private bool waitingForInput = false;
    private MonoBehaviour playerController;
    private Animator playerAnimator;
    private EarthBossAI bossAI;

    private string[][] dialogueLines = new string[][]
    {
        // { speaker name, text, color hex }
        new string[] { "EARTH GUARDIAN", "...", "8B6914" },
        new string[] { "EARTH GUARDIAN", "Another fool stumbles into my domain.", "8B6914" },
        new string[] { "EARTH GUARDIAN", "I am the Earth Guardian. I command the very ground beneath your feet!", "8B6914" },
        new string[] { "EARTH GUARDIAN", "My shockwaves will shatter your bones...", "8B6914" },
        new string[] { "EARTH GUARDIAN", "And my boulders will rain down from the sky and crush what remains!", "8B6914" },
        new string[] { "EARTH GUARDIAN", "You cannot defeat the power of the earth itself!", "8B6914" },
        new string[] { "EARTH GUARDIAN", "NOW... FACE YOUR END!", "FF4444" },
    };

    private void Start()
    {
        if (player != null)
        {
            // Find the player controller - try common names
            playerController = player.GetComponent<MonoBehaviour>();
            var allComponents = player.GetComponents<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                string typeName = comp.GetType().Name.ToLower();
                if (typeName.Contains("controller") || typeName.Contains("movement") || typeName.Contains("input"))
                {
                    playerController = comp;
                    break;
                }
            }
            playerAnimator = player.GetComponentInChildren<Animator>();
        }

        if (earthBoss != null)
            bossAI = earthBoss.GetComponent<EarthBossAI>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);
    }

    public void StartCutscene()
    {
        if (cutsceneActive) return;
        cutsceneActive = true;
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        Debug.Log("Cutscene started!");

        // Disable player movement
        DisablePlayerControl();

        // Stop boss AI from activating early
        if (bossAI != null)
            bossAI.enabled = false;

        // Stop rocks during cutscene
        if (rockSpawner != null)
            rockSpawner.StopSpawning();

        yield return new WaitForSeconds(0.3f);

        // === AUTO-WALK PLAYER INTO ARENA ===
        if (playerWalkTarget != null && player != null)
        {
            Vector3 start = player.transform.position;
            Vector3 end = playerWalkTarget.position;
            float distance = Vector3.Distance(start, end);
            float duration = distance / playerWalkSpeed;
            float elapsed = 0f;

            // Figure out walk direction
            Vector3 dir = (end - start).normalized;

            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("MoveX", dir.x);
                playerAnimator.SetFloat("MoveY", dir.y);
                playerAnimator.SetBool("IsMoving", true);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                player.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            player.transform.position = end;

            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsMoving", false);
                // Face up toward boss
                playerAnimator.SetFloat("MoveX", 0f);
                playerAnimator.SetFloat("MoveY", 1f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // === BOSS SHAKES THE GROUND ===
        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.3f, 0.08f);

        yield return new WaitForSeconds(0.4f);

        // === SHOW DIALOGUE ===
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            string speaker = dialogueLines[i][0];
            string text = dialogueLines[i][1];
            string colorHex = dialogueLines[i][2];

            // Set speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = speaker;
                Color nameColor;
                if (ColorUtility.TryParseHtmlString("#" + colorHex, out nameColor))
                    speakerNameText.color = nameColor;
            }

            // Boss shakes ground on certain lines
            if (i == 3 || i == 4 || i == 6) // shockwave, boulders, NOW FACE YOUR END
            {
                if (shake != null)
                    shake.Shake(0.2f, 0.06f);
            }

            // The "..." line - boss notices you
            if (i == 0)
            {
                yield return StartCoroutine(TypeText(text));
                yield return new WaitForSeconds(1.0f);
                continue; // auto-advance the "..."
            }

            // Last line - dramatic red text
            if (i == dialogueLines.Length - 1)
            {
                if (dialogueText != null)
                    dialogueText.color = new Color(1f, 0.3f, 0.3f);
            }
            else
            {
                if (dialogueText != null)
                    dialogueText.color = Color.white;
            }

            // Type out the text
            yield return StartCoroutine(TypeText(text));

            // Wait for input
            waitingForInput = true;
            while (waitingForInput)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)
                    || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                {
                    waitingForInput = false;
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        // === HIDE DIALOGUE ===
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        // === BIG CAMERA SHAKE — FIGHT STARTS ===
        if (shake != null)
            shake.Shake(0.4f, 0.1f);

        yield return new WaitForSeconds(0.5f);

        // Show boss health bar
        if (bossHealthBar != null)
            bossHealthBar.SetActive(true);

        // Enable boss AI
        if (bossAI != null)
            bossAI.enabled = true;

        // Enable player control
        EnablePlayerControl();

        // Start rocks
        if (rockSpawner != null)
            rockSpawner.StartSpawning();
        
        if (pillarSpawner != null)
            pillarSpawner.StartSpawning();

        cutsceneActive = false;
        Debug.Log("Cutscene complete — FIGHT BEGINS!");
    }

    private IEnumerator TypeText(string fullText)
    {
        if (dialogueText == null) yield break;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            // Skip ahead if player holds space
            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                dialogueText.text = fullText;
                yield return null;
                yield break;
            }

            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void DisablePlayerControl()
    {
        if (player == null) return;

        // Disable ALL MonoBehaviours that handle movement
        var components = player.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            string name = comp.GetType().Name.ToLower();
            if (name.Contains("controller") || name.Contains("movement")
                || name.Contains("combat") || name.Contains("ability")
                || name.Contains("input"))
            {
                comp.enabled = false;
            }
        }

        // Also check children
        var childComponents = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var comp in childComponents)
        {
            string name = comp.GetType().Name.ToLower();
            if (name.Contains("controller") || name.Contains("movement")
                || name.Contains("ability"))
            {
                comp.enabled = false;
            }
        }

        // Stop rigidbody
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void EnablePlayerControl()
    {
        if (player == null) return;

        var components = player.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            string name = comp.GetType().Name.ToLower();
            if (name.Contains("controller") || name.Contains("movement")
                || name.Contains("combat") || name.Contains("ability")
                || name.Contains("input"))
            {
                comp.enabled = true;
            }
        }

        var childComponents = player.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var comp in childComponents)
        {
            string name = comp.GetType().Name.ToLower();
            if (name.Contains("controller") || name.Contains("movement")
                || name.Contains("ability"))
            {
                comp.enabled = true;
            }
        }
    }
}