using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public Image fire;
    public Image water;
    public Image earth;
    public Image lightning;

    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color unlockedColor = Color.white;

    [Header("Key Labels (auto-created if empty)")]
    [SerializeField] private int keyLabelFontSize = 14;
    [SerializeField] private Vector2 keyLabelOffset = new Vector2(0f, -55f);

    private bool labelsCreated;

    void Start()
    {
        CreateKeyLabels();
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void CreateKeyLabels()
    {
        if (labelsCreated) return;
        labelsCreated = true;

        CreateLabel(fire,      "J");
        CreateLabel(water,     "K");
        CreateLabel(earth,     "L-Click");
        CreateLabel(lightning, "L-Shift");
    }

    void CreateLabel(Image icon, string keyText)
    {
        if (icon == null) return;

        GameObject labelObj = new GameObject("KeyLabel");
        labelObj.transform.SetParent(icon.transform, false);

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = keyText;
        tmp.fontSize = keyLabelFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = keyLabelOffset;
        rt.sizeDelta = new Vector2(80f, 20f);
    }

    void UpdateUI()
    {
        bool fireUnlocked = GameProgressManager.Instance.HasElement(ElementType.Fire);
        bool waterUnlocked = GameProgressManager.Instance.HasElement(ElementType.Water);
        bool earthUnlocked = GameProgressManager.Instance.HasElement(ElementType.Earth);
        bool lightningUnlocked = GameProgressManager.Instance.HasElement(ElementType.Lightning);

        fire.color = fireUnlocked ? unlockedColor : lockedColor;
        fire.transform.localScale = fireUnlocked ? Vector3.one * 1.1f : Vector3.one;

        water.color = waterUnlocked ? unlockedColor : lockedColor;
        water.transform.localScale = waterUnlocked ? Vector3.one * 1.1f : Vector3.one;

        earth.color = earthUnlocked ? unlockedColor : lockedColor;
        earth.transform.localScale = earthUnlocked ? Vector3.one * 1.1f : Vector3.one;

        lightning.color = lightningUnlocked ? unlockedColor : lockedColor;
        lightning.transform.localScale = lightningUnlocked ? Vector3.one * 1.1f : Vector3.one;
    }
}