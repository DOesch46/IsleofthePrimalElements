using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour, IInteractable
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string transitionId;

    [Header("Prompt")]
    [SerializeField] private string promptMessage = "Press E to enter";

    public bool IsInteractable => true;

    public string GetPromptText() => promptMessage;

    public void Interact(PlayerController player)
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager not found!");
            return;
        }

        SceneTransitionManager.Instance.TransitionToScene(targetSceneName, transitionId);
    }
}