using UnityEngine;

public class PlayerWaterAbility : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
    }

    public void BeginCharge()
    {
        playerCombat?.BeginWaveCharge();
    }

    public void ReleaseCharge()
    {
        playerCombat?.ReleaseWaveCharge();
    }

    public void CancelCharge()
    {
        playerCombat?.CancelWaveCharge();
    }
}
