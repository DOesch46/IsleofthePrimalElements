using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    private MaterialPropertyBlock mpb;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        if (sr == null) yield break;

        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        sr.color = originalColor;
    }
}