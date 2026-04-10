using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    private const string InteractHintObjectName = "InteractHintLabel";
    private const string AttackHintObjectName = "AttackHintLabel";

    public Image fire;
    public Image water;
    public Image earth;
    public Image lightning;

    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color unlockedColor = Color.white;

    [Header("Key Labels (auto-created if empty)")]
    [SerializeField] private int keyLabelFontSize = 14;
    [SerializeField] private Vector2 keyLabelOffset = new Vector2(0f, -55f);
    [SerializeField] private string fireKeyText = "Click";
    [SerializeField] private string waterKeyText = "Q";
    [SerializeField] private string earthKeyText = "T";
    [SerializeField] private string lightningKeyText = "L-Shift";

    [Header("Action Hints (auto-created if empty)")]
    [SerializeField] private int actionHintFontSize = 18;
    [SerializeField] private Vector2 attackHintOffset = new Vector2(250f, 30f);
    [SerializeField] private Vector2 interactHintOffset = new Vector2(250f, 0f);
    [SerializeField] private string attackHintText = "attack = space";
    [SerializeField] private string interactHintText = "interact = e";

    private bool labelsCreated;

    void Start()
    {
        CreateKeyLabels();
        CreateActionHints();
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

        CreateLabel(fire, fireKeyText);
        CreateLabel(water, waterKeyText);
        CreateLabel(earth, earthKeyText);
        CreateLabel(lightning, lightningKeyText);
    }

    void CreateLabel(Image icon, string keyText)
    {
        if (icon == null || string.IsNullOrWhiteSpace(keyText)) return;

        Transform existing = icon.transform.Find("KeyLabel");
        if (existing != null)
        {
            TextMeshProUGUI existingLabel = existing.GetComponent<TextMeshProUGUI>();
            if (existingLabel != null)
                existingLabel.text = keyText;
            return;
        }

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

    void CreateActionHints()
    {
        CreateHintLabel(InteractHintObjectName, interactHintText, interactHintOffset);
        CreateHintLabel(AttackHintObjectName, attackHintText, attackHintOffset);
    }

    void CreateHintLabel(string objectName, string hintText, Vector2 anchoredPosition)
    {
        if (string.IsNullOrWhiteSpace(hintText)) return;
        if (GetComponent<RectTransform>() == null) return;

        Transform existing = transform.Find(objectName);
        if (existing != null)
        {
            TextMeshProUGUI existingText = existing.GetComponent<TextMeshProUGUI>();
            if (existingText != null)
                existingText.text = hintText;
            return;
        }

        GameObject labelObj = new GameObject(objectName);
        labelObj.transform.SetParent(transform, false);

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = hintText;
        tmp.fontSize = actionHintFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(220f, 24f);
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
