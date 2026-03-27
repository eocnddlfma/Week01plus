#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Editor script to generate prefabs for special orbital weapons
/// </summary>
public class OrbitalWeaponPrefabGenerator
{
    private static readonly string PREFAB_PATH = "Assets/03. Prefab/Satellites/";
    private static readonly string BASE_SATELLITE_PATH = "Assets/03. Prefab/Satellites/BaseSatellite.prefab";

    private static List<(string name, string description, System.Type weaponType)> ORBITAL_WEAPONS = new()
    {
        ("BatOrbitalWeapon", "빠따공: 평소는 일반 공, 배트 휘두를 때 자신의 위치에서 또 다른 배트를 휘둘러서 공격", typeof(BatOrbitalWeapon)),
        ("WallOrbitalWeapon", "벽공: 적과 부착되어 같이 움직임", typeof(WallOrbitalWeapon)),
        ("BombOrbitalWeapon", "폭탄공: 적 접촉 시 범위 폭발", typeof(BombOrbitalWeapon)),
        ("PenetrationOrbitalWeapon", "관통공: 적 접촉 시 가속하며 관통", typeof(PenetrationOrbitalWeapon)),
        ("BounceOrbitalWeapon", "튕기는 공: 벽에 반사되며, 돌아올 때는 벽 무시", typeof(BounceOrbitalWeapon)),
    };

    [MenuItem("Tools/Generate Special Orbital Weapon Prefabs")]
    public static void GenerateOrbitalWeaponPrefabs()
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BASE_SATELLITE_PATH);
        if (basePrefab == null)
        {
            Debug.LogError("BaseSatellite prefab not found!");
            return;
        }

        foreach (var (name, description, weaponType) in ORBITAL_WEAPONS)
        {
            CreateOrbitalWeaponPrefab(basePrefab, name, description, weaponType);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all special orbital weapon prefabs!");
    }

    private static void CreateOrbitalWeaponPrefab(GameObject basePrefab, string weaponName, string description, System.Type weaponType)
    {
        // Instantiate the base prefab
        GameObject instance = Object.Instantiate(basePrefab);
        instance.name = weaponName;

        // Remove the old OrbitalWeapon component
        OrbitalWeapon oldWeapon = instance.GetComponent<OrbitalWeapon>();
        if (oldWeapon != null)
            Object.DestroyImmediate(oldWeapon);

        // Add the new specific orbital weapon component
        instance.AddComponent(weaponType);

        // Update SatelliteData using reflection (fields are private)
        SatelliteData satelliteData = instance.GetComponent<SatelliteData>();
        if (satelliteData != null)
        {
            FieldInfo nameField = typeof(SatelliteData).GetField("satelliteName", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo descField = typeof(SatelliteData).GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo rarityField = typeof(SatelliteData).GetField("rarity", BindingFlags.NonPublic | BindingFlags.Instance);

            if (nameField != null) nameField.SetValue(satelliteData, weaponName);
            if (descField != null) descField.SetValue(satelliteData, description);
            if (rarityField != null) rarityField.SetValue(satelliteData, Rarity.희귀);
        }

        // Save as prefab
        string prefabPath = PREFAB_PATH + weaponName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

        // Clean up
        Object.DestroyImmediate(instance);

        Debug.Log($"Created prefab: {prefabPath}");
    }
}
#endif
