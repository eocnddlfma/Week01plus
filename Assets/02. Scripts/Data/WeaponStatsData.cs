using UnityEngine;

[CreateAssetMenu(menuName = "Game/WeaponStats")]
public class WeaponStatsData : ScriptableObject
{
    [Header("차지")]
    public float chargeCooldown = 0.3f;
    public float maxChargeTime = 1.2f;
    public float chargeThreshold = 0.2f;
    public float orbitalChargeSpeedBoostMax = 2f;
    public float orbitalChargeClusterStrength = 2f;

    [Header("차지 레벨 (4단계)")]
    public ChargeLevelData[] chargeLevels = new ChargeLevelData[4];

    [System.Serializable]
    public class ChargeLevelData
    {
        public string levelName;
        public float rotationAngle = 180f;
        public float attackPower = 1f;
        public float rotationDuration = 0.3f;
        public Color weaponColor = Color.white;
        public float weaponSizeMultiplier = 1f;
        public float knockbackForce = 5f;
        public int damageAmount = 1;
        public float hitStopDurationMult = 1f;
    }
}
