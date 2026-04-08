using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    void Update()
    {
coinText.text = GameProgressManager.Instance.GetCoins().ToString();
    }
}