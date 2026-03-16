using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class UpgradeDataGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Default Upgrades")]
    public static void GenerateDefaultUpgrades()
    {
        GenerateUpgrades();
    }

    [MenuItem("Tools/Generate Ball Data")]
    public static void GenerateBallData()
    {
        GenerateBalls();
    }

    [MenuItem("Tools/Generate All (Upgrades + Balls)")]
    public static void GenerateAll()
    {
        GenerateUpgrades();
        GenerateBalls();
    }

    private static void GenerateUpgrades()
    {
        string folderPath = "Assets/00. SO/Upgrades";

        // 폴더 생성
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/00. SO"))
                AssetDatabase.CreateFolder("Assets", "00. SO");
            AssetDatabase.CreateFolder("Assets/00. SO", "Upgrades");
        }

        // 기본 강화들 (prerequisite: 선행 조건 업그레이드 이름들)
        var upgrades = new[]
        {
            new UpgradeInfo("이동속도", "플레이어 이동 속도 증가", UpgradeStatType.MoveSpeed, 0.15f, UpgradeRarity.Common, null),
            new UpgradeInfo("이동속도 II", "플레이어 이동 속도 증가 (상위)", UpgradeStatType.MoveSpeed, 0.10f, UpgradeRarity.Rare, new[] { "이동속도" }),

            new UpgradeInfo("배트 데미지", "배트 공격력 증가", UpgradeStatType.BatDamage, 0.2f, UpgradeRarity.Common, null),
            new UpgradeInfo("배트 데미지 II", "배트 공격력 증가 (상위)", UpgradeStatType.BatDamage, 0.15f, UpgradeRarity.Rare, new[] { "배트 데미지" }),

            new UpgradeInfo("공 데미지", "공 공격력 증가", UpgradeStatType.BallDamage, 0.2f, UpgradeRarity.Common, null),
            new UpgradeInfo("공 데미지 II", "공 공격력 증가 (상위)", UpgradeStatType.BallDamage, 0.15f, UpgradeRarity.Rare, new[] { "공 데미지" }),

            new UpgradeInfo("넉백", "적 넉백 증가", UpgradeStatType.Knockback, 0.2f, UpgradeRarity.Common, null),
            new UpgradeInfo("넉백 II", "적 넉백 증가 (상위)", UpgradeStatType.Knockback, 0.15f, UpgradeRarity.Rare, new[] { "넉백" }),

            new UpgradeInfo("차지 속도", "배트 차지 속도 증가", UpgradeStatType.ChargeSpeed, 0.15f, UpgradeRarity.Common, null),
            new UpgradeInfo("차지 속도 II", "배트 차지 속도 증가 (상위)", UpgradeStatType.ChargeSpeed, 0.1f, UpgradeRarity.Rare, new[] { "차지 속도" }),

            new UpgradeInfo("공 발사 속도", "공 발사 속도 증가", UpgradeStatType.BallSpeed, 0.15f, UpgradeRarity.Common, null),
            new UpgradeInfo("공 발사 속도 II", "공 발사 속도 증가 (상위)", UpgradeStatType.BallSpeed, 0.1f, UpgradeRarity.Rare, new[] { "공 발사 속도" }),

            new UpgradeInfo("공격 범위", "배트 공격 범위 증가", UpgradeStatType.AttackRange, 0.15f, UpgradeRarity.Common, null),
            new UpgradeInfo("공격 범위 II", "배트 공격 범위 증가 (상위)", UpgradeStatType.AttackRange, 0.1f, UpgradeRarity.Rare, new[] { "공격 범위" }),

            new UpgradeInfo("대시 쿨다운", "대시 쿨다운 감소", UpgradeStatType.DashCooldown, -0.2f, UpgradeRarity.Common, null),
            new UpgradeInfo("대시 쿨다운 II", "대시 쿨다운 감소 (상위)", UpgradeStatType.DashCooldown, -0.15f, UpgradeRarity.Rare, new[] { "대시 쿨다운" }),

            new UpgradeInfo("무적 시간", "피격 후 무적 시간 증가", UpgradeStatType.Invincibility, 0.2f, UpgradeRarity.Rare, null),
            new UpgradeInfo("무적 시간 II", "피격 후 무적 시간 증가 (상위)", UpgradeStatType.Invincibility, 0.15f, UpgradeRarity.Epic, new[] { "무적 시간" }),

            new UpgradeInfo("최대 체력", "최대 체력 증가", UpgradeStatType.MaxHp, 10f, UpgradeRarity.Common, null),
            new UpgradeInfo("최대 체력 II", "최대 체력 증가 (상위)", UpgradeStatType.MaxHp, 10f, UpgradeRarity.Rare, new[] { "최대 체력" }),

            new UpgradeInfo("초당 회복", "초당 체력 0.5 회복", UpgradeStatType.HpRegen, 0.5f, UpgradeRarity.Rare, new[] { "최대 체력" }),
            new UpgradeInfo("초당 회복 II", "초당 체력 1.0 회복", UpgradeStatType.HpRegen, 1f, UpgradeRarity.Epic, new[] { "초당 회복" }),

            new UpgradeInfo("빠따 휘두르기", "배트 공격 속도 증가", UpgradeStatType.BatAttackSpeed, 0.2f, UpgradeRarity.Common, null),
            new UpgradeInfo("빠따 휘두르기 II", "배트 공격 속도 증가 (상위)", UpgradeStatType.BatAttackSpeed, 0.15f, UpgradeRarity.Rare, new[] { "빠따 휘두르기" }),

            new UpgradeInfo("빠따 쿨다운", "배트 공격 쿨다운 감소", UpgradeStatType.BatAttackCooldown, -0.15f, UpgradeRarity.Common, null),
            new UpgradeInfo("빠따 쿨다운 II", "배트 공격 쿨다운 감소 (상위)", UpgradeStatType.BatAttackCooldown, -0.1f, UpgradeRarity.Rare, new[] { "빠따 쿨다운" }),
        };

        // 1단계: 모든 업그레이드 생성
        var createdAssets = new System.Collections.Generic.Dictionary<string, UpgradeData>();
        int created = 0;

        foreach (var info in upgrades)
        {
            string assetPath = $"{folderPath}/{info.name}.asset";

            // 이미 존재하면 로드
            UpgradeData existingData = AssetDatabase.LoadAssetAtPath<UpgradeData>(assetPath);
            if (existingData != null)
            {
                Debug.Log($"[스킵] 이미 존재: {info.name}");
                createdAssets[info.name] = existingData;
                continue;
            }

            UpgradeData data = ScriptableObject.CreateInstance<UpgradeData>();
            data.upgradeName = info.name;
            data.description = info.description;
            data.statType = info.statType;
            data.value = info.value;
            data.rarity = info.rarity;

            AssetDatabase.CreateAsset(data, assetPath);
            createdAssets[info.name] = data;
            created++;
        }

        // 2단계: prerequisites 설정
        foreach (var info in upgrades)
        {
            if (info.prerequisites == null || info.prerequisites.Length == 0)
                continue;

            string assetPath = $"{folderPath}/{info.name}.asset";
            UpgradeData data = AssetDatabase.LoadAssetAtPath<UpgradeData>(assetPath);

            if (data != null)
            {
                data.prerequisites = new System.Collections.Generic.List<UpgradeData>();
                foreach (var prereqName in info.prerequisites)
                {
                    if (createdAssets.TryGetValue(prereqName, out UpgradeData prereqData))
                    {
                        data.prerequisites.Add(prereqData);
                    }
                    else
                    {
                        Debug.LogWarning($"[경고] {info.name}의 선행 조건 '{prereqName}'을 찾을 수 없습니다.");
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[완료] {created}개의 강화 SO 생성됨");
    }

    private static void GenerateBalls()
    {
        string ballsFolderPath = "Assets/00. SO/Balls";

        // 폴더 생성
        if (!AssetDatabase.IsValidFolder(ballsFolderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/00. SO"))
                AssetDatabase.CreateFolder("Assets", "00. SO");
            AssetDatabase.CreateFolder("Assets/00. SO", "Balls");
        }

        // 공 프리팹 찾기
        string[] prefabPaths = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets" });
        int created = 0;

        foreach (var guid in prefabPaths)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // SatelliteData 컴포넌트가 있는 것만 처리
            if (prefab == null || prefab.GetComponent<SatelliteData>() == null)
                continue;

            SatelliteData satelliteData = prefab.GetComponent<SatelliteData>();
            string ballName = satelliteData.SatelliteName;

            string assetPath = $"{ballsFolderPath}/{ballName}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<BallData>(assetPath) != null)
            {
                Debug.Log($"[스킵] 이미 존재: {ballName}");
                continue;
            }

            BallData ballData = ScriptableObject.CreateInstance<BallData>();
            ballData.name = ballName;

            // 리플렉션으로 private 필드 설정 (SatelliteData에서 가져옴)
            var nameField = typeof(BallData).GetField("ballName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = typeof(BallData).GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prefabField = typeof(BallData).GetField("ballPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rarityField = typeof(BallData).GetField("rarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            nameField?.SetValue(ballData, satelliteData.SatelliteName);
            descField?.SetValue(ballData, satelliteData.Description);
            prefabField?.SetValue(ballData, prefab);
            rarityField?.SetValue(ballData, satelliteData.Rarity);

            AssetDatabase.CreateAsset(ballData, assetPath);
            created++;
            Debug.Log($"[생성] BallData: {ballName} (희귀도: {satelliteData.Rarity})");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[완료] {created}개의 BallData SO 생성됨");
    }

    private struct UpgradeInfo
    {
        public string name;
        public string description;
        public UpgradeStatType statType;
        public float value;
        public UpgradeRarity rarity;
        public string[] prerequisites;

        public UpgradeInfo(string name, string description, UpgradeStatType statType, float value, UpgradeRarity rarity, string[] prerequisites = null)
        {
            this.name = name;
            this.description = description;
            this.statType = statType;
            this.value = value;
            this.rarity = rarity;
            this.prerequisites = prerequisites;
        }
    }
}
#endif
