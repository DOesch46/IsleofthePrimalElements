using UnityEngine;
using System.Collections;

public class LeverDrain : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] flipFrames;
    [SerializeField] private float frameTime = 0.08f;
    [SerializeField] private GameObject[] objectsToDisable;

    private bool playerInRange = false;
    private bool activated = false;

    private void Update()
    {
        if (playerInRange && !activated && Input.GetKeyDown(KeyCode.E))
        {
            activated = true;
            StartCoroutine(PlayLeverFlip());

            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private IEnumerator PlayLeverFlip()
    {
        for (int i = 0; i < flipFrames.Length; i++)
        {
            spriteRenderer.sprite = flipFrames[i];
            yield return new WaitForSeconds(frameTime);
        }
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