using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lightweight singleton that persists across scenes.
/// Stores the transition ID so the destination scene knows where to spawn the player.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private string lastTransitionId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads the target scene and stores the transition ID for spawn point matching.
    /// </summary>
    public void TransitionToScene(string sceneName, string transitionId)
    {
        lastTransitionId = transitionId;
        SceneManager.LoadScene(sceneName);
    }

    public string GetLastTransitionId() => lastTransitionId;
}