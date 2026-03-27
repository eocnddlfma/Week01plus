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
            new UpgradeInfo("이동속도", "이동 속도가 50% 증가합니다.", UpgradeStatType.MoveSpeed, 0.5f, UpgradeRarity.Common, null),

            new UpgradeInfo("배트 데미지", "빠따가 200% 더 피해를 줍니다.", UpgradeStatType.BatDamage, 2f, UpgradeRarity.Common, null),

            new UpgradeInfo("공 데미지", "공이 200% 더 피해를 줍니다.", UpgradeStatType.BallDamage, 2f, UpgradeRarity.Common, null),

            new UpgradeInfo("넉백", "넉백량이 100% 증가합니다.", UpgradeStatType.Knockback, 1f, UpgradeRarity.Common, null),

            new UpgradeInfo("차지 속도", "배트 차지속도가 절반으로 감소합니다.", UpgradeStatType.ChargeSpeed, 1f, UpgradeRarity.Common, null),
            new UpgradeInfo("차지 속도 II", "배트 차지속도가 1/4으로 감소합니다", UpgradeStatType.ChargeSpeed, 2f, UpgradeRarity.Rare, new[] { "차지 속도" }),
            new UpgradeInfo("차지 속도 III", "배트 차지속도가 1/8으로 감소합니다", UpgradeStatType.ChargeSpeed, 4f, UpgradeRarity.Epic, new[] { "차지 속도 II" }),

            new UpgradeInfo("공 발사 속도", "공이 50% 더 빨리 날아갑니다.", UpgradeStatType.BallSpeed, 0.5f, UpgradeRarity.Common, null),

            new UpgradeInfo("공격 범위", "배트 공격 범위가 50% 증가합니다.", UpgradeStatType.AttackRange, 0.5f, UpgradeRarity.Common, null),

            new UpgradeInfo("대시 쿨다운", "대시 쿨다운이 0.2초 감소합니다.", UpgradeStatType.DashCooldown, -0.2f, UpgradeRarity.Common, null),

            new UpgradeInfo("무적 시간", "무적시간이 0.2초 증가합니다.", UpgradeStatType.Invincibility, 0.2f, UpgradeRarity.Rare, null),

            new UpgradeInfo("최대 체력", "최대 체력이 30 증가합니다.", UpgradeStatType.MaxHp, 30f, UpgradeRarity.Common, null),

            new UpgradeInfo("초당 회복", "초당 체력 0.15 회복", UpgradeStatType.HpRegen, 0.15f, UpgradeRarity.Rare, new[] { "최대 체력" }),

            new UpgradeInfo("빠따 휘두르기", "배트를 휘두르는 속도가 50% 빨라집니다.", UpgradeStatType.BatAttackSpeed, 0.5f, UpgradeRarity.Common, null),

            new UpgradeInfo("빠따 쿨다운", "배트 쿨다운이 50% 감소합니다.", UpgradeStatType.BatAttackCooldown, 0.5f, UpgradeRarity.Common, null),

            // ── 분열공 ──
            new UpgradeInfo("사방미인", "[분열공] 충돌 시 4방향으로 분열합니다.", UpgradeStatType.Split4Way, 4f, UpgradeRarity.Rare, null),
            new UpgradeInfo("팔방미인", "[분열공] 충돌 시 8방향으로 분열합니다.", UpgradeStatType.Split8Way, 8f, UpgradeRarity.Epic, new[] { "사방미인" }),

            // ── 폭탄공 ──
            new UpgradeInfo("폭탄 받아라", "[폭탄공] 폭발 범위와 데미지가 2배 증가합니다.", UpgradeStatType.BombRadiusMult, 2f, UpgradeRarity.Rare, null),
            new UpgradeInfo("터져버렷", "[폭탄공] 0.5초마다 자동으로 폭발합니다.", UpgradeStatType.BombAutoExplode, 1f, UpgradeRarity.Epic, new[] { "폭탄 받아라" }),

            // ── 빠따공 ──
            new UpgradeInfo("홈런", "[빠따공] 배트 스윙 범위가 2배 증가합니다.", UpgradeStatType.BatSwingRadius, 2f, UpgradeRarity.Rare, null),
            new UpgradeInfo("빠따로 맞아볼래", "[빠따공] 스윙 범위 내 적(보스 제외)의 현재 체력을 절반으로 만듭니다.", UpgradeStatType.BatHalfHp, 1f, UpgradeRarity.Epic, new[] { "홈런" }),

            // ── 벽반사공 ──
            new UpgradeInfo("작용반작용", "[벽반사공] 벽에 반사될 때 속도가 감소하지 않습니다.", UpgradeStatType.BounceNoDamp, 1f, UpgradeRarity.Rare, null),
            new UpgradeInfo("예술적 각도", "[벽반사공] 벽 반사 횟수에 비례해 충돌 데미지가 10%씩 증가합니다.", UpgradeStatType.BounceHitBonus, 1f, UpgradeRarity.Epic, new[] { "작용반작용" }),

            // ── 중력공 ──
            new UpgradeInfo("내게로 와", "[중력공] 인력이 2배 증가합니다.", UpgradeStatType.GravityMult, 2f, UpgradeRarity.Rare, null),
            new UpgradeInfo("저리가!", "[중력공] 인력이 척력으로 바뀝니다.", UpgradeStatType.GravityRepel, 1f, UpgradeRarity.Epic, new[] { "내게로 와" }),

            // ── 무거운공 ──
            new UpgradeInfo("압사", "[무거운공] 적의 최대 체력의 3%를 추가 데미지로 입힙니다.", UpgradeStatType.HeavyMaxHpDamage, 0.03f, UpgradeRarity.Rare, null),
            new UpgradeInfo("컬링 마스터", "[무거운공] 충돌 시 주변의 다른 공들을 날려보냅니다.", UpgradeStatType.HeavyCurling, 1f, UpgradeRarity.Epic, new[] { "압사" }),

            // ── 평범한공 ──
            new UpgradeInfo("애도", "[평범한공] 데미지가 100으로 고정됩니다.", UpgradeStatType.NormalFixedDamage, 100f, UpgradeRarity.Rare, null),
            new UpgradeInfo("기도", "[평범한공] 데미지가 500으로 고정됩니다.", UpgradeStatType.NormalFixedDamage, 500f, UpgradeRarity.Epic, new[] { "애도" }),
            new UpgradeInfo("회고", "[평범한공] 데미지가 1000으로 고정됩니다.", UpgradeStatType.NormalFixedDamage, 1000f, UpgradeRarity.Epic, new[] { "기도" }),

            // ── 관통공 ──
            new UpgradeInfo("펜싱마스터", "[관통공] 발사 거리가 2배 증가합니다.", UpgradeStatType.PenFencingMaster, 2f, UpgradeRarity.Rare, null),
            new UpgradeInfo("연속찌르기", "[관통공] 0.5초 이내 재충돌 시 1.2배 데미지를 입힙니다.", UpgradeStatType.PenContinuousStab, 1f, UpgradeRarity.Epic, new[] { "펜싱마스터" }),

            // ── 작은공 ──
            new UpgradeInfo("다윗과 골리앗", "[작은공] 데미지가 10배 증가합니다.", UpgradeStatType.SmallDamageMult, 10f, UpgradeRarity.Rare, null),
            new UpgradeInfo("핵앤슬래시", "[작은공] 데미지가 15배로 증가하지만 홀수 번째 충돌은 1/10 데미지입니다.", UpgradeStatType.SmallHackSlash, 15f, UpgradeRarity.Epic, new[] { "다윗과 골리앗" }),

            // ── 직선공 ──
            new UpgradeInfo("찌찌르기!", "[직선공] 복귀 시 한 번 더 자동 발사됩니다.", UpgradeStatType.StraightRelaunch, 1f, UpgradeRarity.Rare, null),
            new UpgradeInfo("직선넘네", "[직선공] 밀쳐내는 힘이 2배 증가합니다.", UpgradeStatType.StraightKnockback, 2f, UpgradeRarity.Epic, new[] { "찌찌르기!" }),

            // ── 벽공 ──
            new UpgradeInfo("벽력일섬", "[벽공] 10명 이상의 적이 달라붙으면 모두 현재 체력의 절반 피해를 입습니다.", UpgradeStatType.WallThresholdBlast, 1f, UpgradeRarity.Rare, null),
            new UpgradeInfo("벽치기", "[벽공] 복귀 시 달라붙은 적들을 날려보냅니다.", UpgradeStatType.WallThrowDetach, 1f, UpgradeRarity.Epic, new[] { "벽력일섬" }),
            new UpgradeInfo("판때기", "[벽공] 공의 가로 크기가 4배 증가합니다.", UpgradeStatType.WallWideBody, 1f, UpgradeRarity.Rare, null),

            // ── 회오리공 ──
            new UpgradeInfo("가출", "[회오리공] 궤도로 돌아오지 않고 계속 날아다닙니다.", UpgradeStatType.WhirlNoReturn, 1f, UpgradeRarity.Rare, null),
            new UpgradeInfo("몰아치기", "[회오리공] 충돌마다 데미지+1, 피격 시 초기화됩니다.", UpgradeStatType.WhirlCollisionStack, 1f, UpgradeRarity.Epic, new[] { "가출" }),

            // ── 차지 단계 해금 (시작은 0단계) ──
            new UpgradeInfo("차지 1단계", "배트 차지 1단계가 해금됩니다. 최대 차지 시간 +0.5초", UpgradeStatType.UnlockChargeLevel, 1f, UpgradeRarity.Rare, null),
            new UpgradeInfo("차지 2단계", "배트 차지 2단계가 해금됩니다. 최대 차지 시간 +0.5초", UpgradeStatType.UnlockChargeLevel, 1f, UpgradeRarity.Rare, new[] { "차지 1단계" }),
            new UpgradeInfo("차지 3단계", "배트 차지 3단계가 해금됩니다. 최대 차지 시간 +0.5초", UpgradeStatType.UnlockChargeLevel, 1f, UpgradeRarity.Epic, new[] { "차지 2단계" }),
        };

        // 1단계: 모든 업그레이드 생성
        var createdAssets = new System.Collections.Generic.Dictionary<string, UpgradeData>();
        int created = 0;

        foreach (var info in upgrades)
        {
            string assetPath = $"{folderPath}/{info.name}.asset";

            // 이미 존재하면 값 업데이트
            UpgradeData existingData = AssetDatabase.LoadAssetAtPath<UpgradeData>(assetPath);
            if (existingData != null)
            {
                existingData.upgradeName = info.name;
                existingData.description = info.description;
                existingData.statType    = info.statType;
                existingData.value       = info.value;
                existingData.rarity      = info.rarity;
                EditorUtility.SetDirty(existingData);
                createdAssets[info.name] = existingData;
                Debug.Log($"[업데이트] {info.name}");
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

            if (!createdAssets.TryGetValue(info.name, out UpgradeData data) || data == null)
            {
                Debug.LogWarning($"[경고] {info.name} 에셋을 찾을 수 없습니다.");
                continue;
            }

            data.prerequisites = new System.Collections.Generic.List<UpgradeData>();
            foreach (var prereqName in info.prerequisites)
            {
                if (createdAssets.TryGetValue(prereqName, out UpgradeData prereqData))
                    data.prerequisites.Add(prereqData);
                else
                    Debug.LogWarning($"[경고] {info.name}의 선행 조건 '{prereqName}'을 찾을 수 없습니다.");
            }

            EditorUtility.SetDirty(data); // 변경 사항 저장 보장
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[완료] {created}개의 강화 SO 생성됨");

        AutoPopulateUpgradeManager();
    }

    private static void AutoPopulateUpgradeManager()
    {
        UpgradeManager manager = FindAnyObjectByType<UpgradeManager>();
        if (manager == null)
        {
            Debug.LogWarning("[UpgradeManager] 씬에서 UpgradeManager를 찾을 수 없습니다. _upgradePool을 수동으로 설정해주세요.");
            return;
        }

        // 폴더의 모든 UpgradeData 로드
        string[] guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets/00. SO/Upgrades" });
        var allUpgrades = new System.Collections.Generic.List<UpgradeData>();
        foreach (var guid in guids)
        {
            UpgradeData data = AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null)
                allUpgrades.Add(data);
        }

        SerializedObject so = new SerializedObject(manager);
        SerializedProperty poolProp = so.FindProperty("_upgradePool");

        // 기존에 이미 있는 것 파악
        var existingSet = new System.Collections.Generic.HashSet<Object>();
        for (int i = 0; i < poolProp.arraySize; i++)
        {
            var elem = poolProp.GetArrayElementAtIndex(i).objectReferenceValue;
            if (elem != null) existingSet.Add(elem);
        }

        int added = 0;
        foreach (var upgrade in allUpgrades)
        {
            if (existingSet.Contains(upgrade)) continue;
            poolProp.arraySize++;
            poolProp.GetArrayElementAtIndex(poolProp.arraySize - 1).objectReferenceValue = upgrade;
            added++;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

        Debug.Log($"[UpgradeManager] {added}개 추가됨 (총 {poolProp.arraySize}개)");
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
