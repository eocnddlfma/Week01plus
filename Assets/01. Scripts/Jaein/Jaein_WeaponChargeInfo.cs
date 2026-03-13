using UnityEngine;

/// <summary>
/// 무기에 부착되어 차지 정보를 저장
/// OrbitalWeapon이 충돌 시 이 정보를 읽어 Launch 강도를 결정
/// </summary>
public class Jaein_WeaponChargeInfo : MonoBehaviour
{
    private float _chargePercent = 0f;

    public float ChargePercent => _chargePercent;

    public void SetChargePercent(float percent)
    {
        _chargePercent = Mathf.Clamp01(percent);
    }

    public void ResetCharge()
    {
        _chargePercent = 0f;
    }
}
