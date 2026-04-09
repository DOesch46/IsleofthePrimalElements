using UnityEngine;

public class EarthTrigger : MonoBehaviour
{
    public EarthCollapse collapse;

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        collapse.StartCollapse();
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        collapse.StopCollapse();
    }
}
}