using UnityEngine;
using System.Collections;

public class PuzzleLever : MonoBehaviour
{
    [SerializeField] private int leverIndex;
    [SerializeField] private LeverPuzzleManager puzzleManager;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] offToOnFrames;
    [SerializeField] private Sprite[] onToOffFrames;
    [SerializeField] private float frameTime = 0.08f;

    private bool playerInRange = false;
    private bool isOn = false;
    private bool isAnimating = false;

    private void Start()
    {
        if (puzzleManager != null)
        {
            puzzleManager.SetLeverState(leverIndex, 0);
        }

        if (spriteRenderer != null && offToOnFrames.Length > 0)
        {
            spriteRenderer.sprite = offToOnFrames[0];
        }
    }

    private void Update()
    {
        if (playerInRange && !isAnimating && Input.GetKeyDown(KeyCode.E))
        {
            ToggleLever();
        }
    }

    private void ToggleLever()
    {
        isOn = !isOn;

        if (puzzleManager != null)
        {
            puzzleManager.SetLeverState(leverIndex, isOn ? 1 : 0);
        }

        if (isOn)
        {
            StartCoroutine(PlayFrames(offToOnFrames));
        }
        else
        {
            StartCoroutine(PlayFrames(onToOffFrames));
        }
    }

    private IEnumerator PlayFrames(Sprite[] frames)
    {
        isAnimating = true;

        if (spriteRenderer != null && frames != null && frames.Length > 0)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                spriteRenderer.sprite = frames[i];
                yield return new WaitForSeconds(frameTime);
            }
        }

        isAnimating = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}