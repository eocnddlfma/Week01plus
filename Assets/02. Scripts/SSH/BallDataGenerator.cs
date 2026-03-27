#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor script to generate BallData ScriptableObjects for special orbital weapons
/// Requires: Prefabs to be created in Assets/03. Prefab/Satellites/
/// </summary>
public class BallDataGenerator
{
    private static readonly string BALLDATA_PATH = "Assets/00. SO/Balls/";
    private static readonly string PREFAB_PATH = "Assets/03. Prefab/Satellites/";

    private static List<(string prefabName, string ballName, string description, Rarity rarity)> BALL_DATA = new()
    {
        ("BatOrbitalWeapon.prefab", "빠따공", "평소는 일반 공, 배트 휘두를 때 자신의 위치에서 또 다른 배트를 휘둘러서 공격", Rarity.희귀),
        ("WallOrbitalWeapon.prefab", "벽공", "적과 부착되어 같이 움직임", Rarity.희귀),
        ("BombOrbitalWeapon.prefab", "폭탄공", "적 접촉 시 범위 폭발", Rarity.희귀),
        ("PenetrationOrbitalWeapon.prefab", "관통공", "적 접촉 시 가속하며 관통", Rarity.희귀),
        ("BounceOrbitalWeapon.prefab", "튕기는 공", "벽에 반사되며, 돌아올 때는 벽 무시", Rarity.희귀),
    };

    [MenuItem("Tools/Generate BallData Assets")]
    public static void GenerateBallDataAssets()
    {
        // Ensure directory exists
        if (!System.IO.Directory.Exists(BALLDATA_PATH))
        {
            System.IO.Directory.CreateDirectory(BALLDATA_PATH);
        }

        foreach (var (prefabName, ballName, description, rarity) in BALL_DATA)
        {
            CreateBallData(prefabName, ballName, description, rarity);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all BallData assets!");
    }

    private static void CreateBallData(string prefabName, string ballName, string description, Rarity rarity)
    {
        // Load the prefab
        string prefabPath = PREFAB_PATH + prefabName;
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (ballPrefab == null)
        {
            Debug.LogWarning($"Prefab not found: {prefabPath}. Skipping...");
            return;
        }

        // Create BallData asset
        BallData ballData = ScriptableObject.CreateInstance<BallData>();

        // Use reflection to set private fields
        System.Reflection.FieldInfo ballNameField = typeof(BallData).GetField("ballName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo descField = typeof(BallData).GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo prefabField = typeof(BallData).GetField("ballPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo rarityField = typeof(BallData).GetField("rarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (ballNameField != null) ballNameField.SetValue(ballData, ballName);
        if (descField != null) descField.SetValue(ballData, description);
        if (prefabField != null) prefabField.SetValue(ballData, ballPrefab);
        if (rarityField != null) rarityField.SetValue(ballData, rarity);

        // Save asset
        string assetPath = BALLDATA_PATH + ballName + ".asset";
        AssetDatabase.CreateAsset(ballData, assetPath);

        Debug.Log($"Created BallData asset: {assetPath}");
    }
}
#endif
