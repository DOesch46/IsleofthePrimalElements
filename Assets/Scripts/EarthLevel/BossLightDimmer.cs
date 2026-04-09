using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossLightDimmer : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private EarthBossHealth bossHealth;
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float rageIntensity = 0.5f;

    private void Update()
    {
        if (globalLight == null || bossHealth == null) return;

        float healthPercent = bossHealth.GetCurrentHealth() / bossHealth.GetMaxHealth();
        float targetIntensity = Mathf.Lerp(rageIntensity, normalIntensity, healthPercent);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime);
    }
}