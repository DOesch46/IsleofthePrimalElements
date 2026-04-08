using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 🔥 makes UI persist
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
