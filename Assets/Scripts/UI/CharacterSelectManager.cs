using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the character selection screen. Handles animated character previews,
/// selection highlighting, and transitioning to the game scene with the chosen character.
///
/// Setup in Unity:
/// 1. Create a Canvas with two Image objects for the characters (BlueCharImage, RedCharImage).
/// 2. Add a Button component to each character Image so they are clickable.
/// 3. Create a highlight Image behind/around each character (yellow border or glow).
/// 4. Add a "Play" Button at the bottom.
/// 5. Drag all references into this script's inspector fields.
/// 6. Assign 3 sprites per character for the idle animation frames.
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector References
    // -------------------------------------------------------------------------

    [Header("Character Images")]
    [SerializeField] private Image blueCharImage;
    [SerializeField] private Image redCharImage;

    [Header("Blue Character Frames (3 sprites)")]
    [SerializeField] private Sprite[] blueFrames;

    [Header("Red Character Frames (3 sprites)")]
    [SerializeField] private Sprite[] redFrames;

    [Header("Selection Highlights")]
    [SerializeField] private GameObject blueHighlight;
    [SerializeField] private GameObject redHighlight;

    [Header("Play Button")]
    [SerializeField] private Button playButton;

    [Header("Audio")]
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip playSound;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float frameInterval = 0.3f;
    [SerializeField] private string gameSceneName = "MainIsland";

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private int selectedCharacter = -1; // -1 = none, 0 = blue, 1 = red
    private float frameTimer;
    private int currentFrame;

    // -------------------------------------------------------------------------
    // Public key used by other scripts to read the chosen character
    // -------------------------------------------------------------------------

    public const string CHARACTER_PREF_KEY = "SelectedCharacter";

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Hide both highlights and disable play button initially
        blueHighlight.SetActive(false);
        redHighlight.SetActive(false);
        playButton.interactable = false;

        // Set initial frames
        if (blueFrames.Length > 0)
            blueCharImage.sprite = blueFrames[0];
        if (redFrames.Length > 0)
            redCharImage.sprite = redFrames[0];
    }

    private void Update()
    {
        AnimateCharacters();
    }

    // -------------------------------------------------------------------------
    // Animation — cycles through 3 frames every frameInterval seconds
    // -------------------------------------------------------------------------

    private void AnimateCharacters()
    {
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameInterval)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % 3;

            if (blueFrames.Length > currentFrame)
                blueCharImage.sprite = blueFrames[currentFrame];

            if (redFrames.Length > currentFrame)
                redCharImage.sprite = redFrames[currentFrame];
        }
    }

    // -------------------------------------------------------------------------
    // Selection — called by Button onClick in the Inspector
    // -------------------------------------------------------------------------

    /// <summary>
    /// Call this from the Blue character Button's OnClick event.
    /// </summary>
    public void SelectBlueCharacter()
    {
        selectedCharacter = 0;
        blueHighlight.SetActive(true);
        redHighlight.SetActive(false);
        playButton.interactable = true;
        if (sfxSource && selectSound) sfxSource.PlayOneShot(selectSound);
    }

    /// <summary>
    /// Call this from the Red character Button's OnClick event.
    /// </summary>
    public void SelectRedCharacter()
    {
        selectedCharacter = 1;
        blueHighlight.SetActive(false);
        redHighlight.SetActive(true);
        playButton.interactable = true;
        if (sfxSource && selectSound) sfxSource.PlayOneShot(selectSound);
    }

    // -------------------------------------------------------------------------
    // Play — called by the Play Button's OnClick event
    // -------------------------------------------------------------------------

    /// <summary>
    /// Saves the selected character to PlayerPrefs and loads the game scene.
    /// </summary>
    public void PlayGame()
    {
        if (selectedCharacter < 0) return;

        if (sfxSource && playSound) sfxSource.PlayOneShot(playSound);

        PlayerPrefs.SetInt(CHARACTER_PREF_KEY, selectedCharacter);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
}
