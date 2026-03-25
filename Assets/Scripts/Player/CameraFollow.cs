using UnityEngine;

/// <summary>
/// Makes the camera follow the player smoothly.
/// Attach this to the Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag the Player here")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (lower = smoother, higher = snappier)")]
    [SerializeField] private float smoothSpeed = 5f;
    
    [Tooltip("Offset from the player (usually just keep Z at -10)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera towards desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Apply position
        transform.position = smoothedPosition;
    }
}