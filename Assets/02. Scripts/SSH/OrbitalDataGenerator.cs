#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to generate OrbitalStatsData ScriptableObjects for special orbital weapons
/// Tools/Generate OrbitalData Assets
/// </summary>
public class OrbitalDataGenerator
{
    private static readonly string ORBITALDATA_PATH = "Assets/00. SO/OrbitalData/";

    [MenuItem("Tools/Generate OrbitalData Assets")]
    public static void GenerateOrbitalDataAssets()
    {
        if (!System.IO.Directory.Exists(ORBITALDATA_PATH))
            System.IO.Directory.CreateDirectory(ORBITALDATA_PATH);

        Create("BatSatellite",         bat =>   { /* 기본값 그대로 */ });
        Create("WallSatellite",        wall =>  { wall.launchSpeed = 10f; wall.launchDuration = 0.5f; });
        Create("BombSatellite",        bomb =>  { bomb.launchSpeed = 12f; bomb.baseDamage = 1; bomb.maxChargeDamage = 5; });
        Create("PenetrationSatellite", pen =>   { pen.launchSpeed = 18f; pen.launchDuration = 0.5f; });
        Create("BounceSatellite",      bounce => { bounce.launchSpeed = 16f; bounce.launchDuration = 0.6f; });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all OrbitalData assets!");
    }

    private static void Create(string assetName, System.Action<OrbitalStatsData> configure)
    {
        string assetPath = ORBITALDATA_PATH + assetName + " Variant.asset";

        OrbitalStatsData data = ScriptableObject.CreateInstance<OrbitalStatsData>();
        configure(data);

        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log($"Created OrbitalData: {assetPath}");
    }
}
#endif
