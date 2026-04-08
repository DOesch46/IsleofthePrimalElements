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

    public bool fireUnlocked = false;
    public bool waterUnlocked = false;
    public bool earthUnlocked = false;
    public bool lightningUnlocked = false;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        fire.color = fireUnlocked ? unlockedColor : lockedColor;
        water.color = waterUnlocked ? unlockedColor : lockedColor;
        earth.color = earthUnlocked ? unlockedColor : lockedColor;
        lightning.color = lightningUnlocked ? unlockedColor : lockedColor;
    }
}