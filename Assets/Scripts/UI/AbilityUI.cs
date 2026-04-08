using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public Image fire;
    public Image water;
    public Image earth;
    public Image lightning;

    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color unlockedColor = Color.white;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
{
    bool fireUnlocked = GameProgressManager.Instance.HasElement(ElementType.Fire);
    bool waterUnlocked = GameProgressManager.Instance.HasElement(ElementType.Water);
    bool earthUnlocked = GameProgressManager.Instance.HasElement(ElementType.Earth);
    bool lightningUnlocked = GameProgressManager.Instance.HasElement(ElementType.Lightning);

    // 🔥 FIRE
    fire.color = fireUnlocked ? unlockedColor : lockedColor;
    fire.transform.localScale = fireUnlocked ? Vector3.one * 1.1f : Vector3.one;

    // 💧 WATER
    water.color = waterUnlocked ? unlockedColor : lockedColor;
    water.transform.localScale = waterUnlocked ? Vector3.one * 1.1f : Vector3.one;

    // 🌱 EARTH
    earth.color = earthUnlocked ? unlockedColor : lockedColor;
    earth.transform.localScale = earthUnlocked ? Vector3.one * 1.1f : Vector3.one;

    // ⚡ LIGHTNING
    lightning.color = lightningUnlocked ? unlockedColor : lockedColor;
    lightning.transform.localScale = lightningUnlocked ? Vector3.one * 1.1f : Vector3.one;
}
}