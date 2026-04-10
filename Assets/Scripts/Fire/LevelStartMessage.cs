using UnityEngine;
using System.Collections;

/// <summary>
/// Shows a guide banner message when the level starts.
/// Attach to any GameObject in the scene.
///
/// SETUP:
/// 1. Create empty GameObject "LevelStartMessage"
/// 2. Attach this script
/// 3. Set the message in Inspector
/// </summary>
public class LevelStartMessage : MonoBehaviour
{
    [Header("Message")]
    [Tooltip("Text shown when the level starts")]
    [SerializeField] private string startMessage = "Ability Granted: Fireball (Mouse Click)";

    [Header("Timing")]
    [Tooltip("Delay before showing the message")]
    [SerializeField] private float delay = 1f;

    private void Start()
    {
        StartCoroutine(ShowStartMessage());
    }

    private IEnumerator ShowStartMessage()
    {
        // Wait for everything to load
        yield return new WaitForSeconds(delay);

        // Show the banner
        if (GuideBanner.Instance != null)
        {
            GuideBanner.Instance.Show(startMessage);
        }
        else
        {
            Debug.LogWarning("LevelStartMessage: No GuideBanner found in scene!");
        }
    }
}