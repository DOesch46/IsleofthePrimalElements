using UnityEngine;
using System.Collections;

public class Challenge3Lever : MonoBehaviour
{
    [Header("Lever Setup")]
    [SerializeField] private GameObject controlledTilemap;
    [SerializeField] private bool startsOn = false;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] leverFrames; // Put frames in normal order: 1,2,3,4,5
    [SerializeField] private float frameTime = 0.08f;

    private bool playerInRange = false;
    private bool isOn = false;
    private bool isAnimating = false;

    private void Start()
    {
        isOn = startsOn;

        if (controlledTilemap != null)
        {
            controlledTilemap.SetActive(isOn);
        }

        if (spriteRenderer != null && leverFrames != null && leverFrames.Length > 0)
        {
            spriteRenderer.sprite = isOn ? leverFrames[leverFrames.Length - 1] : leverFrames[0];
        }
    }

    private void Update()
    {
        if (playerInRange && !isAnimating && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ToggleLever());
        }
    }

    private IEnumerator ToggleLever()
    {
        isAnimating = true;

        if (spriteRenderer == null || leverFrames == null || leverFrames.Length == 0)
        {
            isAnimating = false;
            yield break;
        }

        if (!isOn)
        {
            for (int i = 0; i < leverFrames.Length; i++)
            {
                spriteRenderer.sprite = leverFrames[i];
                yield return new WaitForSeconds(frameTime);
            }

            isOn = true;
        }
        else
        {
            for (int i = leverFrames.Length - 1; i >= 0; i--)
            {
                spriteRenderer.sprite = leverFrames[i];
                yield return new WaitForSeconds(frameTime);
            }

            isOn = false;
        }

        if (controlledTilemap != null)
        {
            controlledTilemap.SetActive(isOn);
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