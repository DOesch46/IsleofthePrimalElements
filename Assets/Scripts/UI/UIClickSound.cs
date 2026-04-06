using UnityEngine;

public class UIClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;

    public void PlayClick()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
        }
    }
}