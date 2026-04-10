using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Shows a guide banner with text at key moments.
/// Player clicks or presses E to dismiss.
///
/// SETUP:
/// 1. Attach to the Banner GameObject under PuzzleCanvas
/// 2. Banner starts hidden, shown by other scripts
/// </summary>
public class GuideBanner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The text on the banner")]
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("Settings")]
    [Tooltip("Can also dismiss by clicking anywhere")]
    [SerializeField] private bool dismissOnClick = true;

    // Singleton for easy access
    public static GuideBanner Instance { get; private set; }

    private bool isShowing = false;

    private void Awake()
    {
        Instance = this;
        // Start hidden
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing) return;

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        // Dismiss on E key
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            Hide();
            return;
        }

        // Dismiss on mouse click
        if (dismissOnClick && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Hide();
            return;
        }
    }

    /// <summary>
    /// Shows the banner with the given message.
    /// Call from anywhere: GuideBanner.Instance.Show("Your message");
    /// </summary>
    public void Show(string message)
    {
        if (guideText != null)
        {
            guideText.text = message;
        }

        gameObject.SetActive(true);
        isShowing = true;

        Debug.Log($"Guide Banner: {message}");
    }

    /// <summary>
    /// Hides the banner.
    /// </summary>
    public void Hide()
    {
        isShowing = false;
        gameObject.SetActive(false);

        Debug.Log("Guide Banner dismissed.");
    }
}