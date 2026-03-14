using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FixMissingScriptsBatch
{
    static readonly string[] prefixes = new string[]
    {
        "Jaein_", "WS_", "Ryeol_", "SSH_", "fbdfbd_"
    };

    [MenuItem("Tools/Fix Missing Scripts (Batch)")]
    static void Fix()
    {
        // 1) 현재 존재하는 모든 MonoBehaviour -> 이름:MonoScript 매핑
        var scriptMap = new Dictionary<string, MonoScript>();
        foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
        {
            var klass = ms.GetClass();
            if (klass == null) continue;
            scriptMap[klass.FullName] = ms;
            scriptMap[klass.Name] = ms;
        }

        // 2) 모든 프리팹 순회
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;
        int failCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var transforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                var go = t.gameObject;
                var so = new SerializedObject(go);
                var componentsProp = so.FindProperty("m_Component");

                for (int i = componentsProp.arraySize - 1; i >= 0; i--)
                {
                    var compRef = componentsProp.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("component");

                    if (compRef.objectReferenceValue != null) continue;

                    // Missing component 발견!
                    // objectReferenceInstanceIDValue로 missing 정보 얻기 시도
                }
            }
        }

        // === 새 접근: Scene/Prefab의 YAML에서 직접 MonoBehaviour 블록 파싱 ===
        FixViaYamlParsing(scriptMap, ref fixedCount, ref failCount);

        AssetDatabase.Refresh();
        Debug.Log($"=== 완료! {fixedCount}개 참조 수정, {failCount}개 매칭 실패 ===");
    }

    static void FixViaYamlParsing(Dictionary<string, MonoScript> scriptMap,
        ref int fixedCount, ref int failCount)
    {
        string[] extensions = new[] { "*.prefab", "*.unity", "*.asset" };

        // 현재 유효한 스크립트 guid 세트
        var validGuids = new HashSet<string>();
        foreach (var ms in scriptMap.Values)
        {
            string p = AssetDatabase.GetAssetPath(ms);
            string g = AssetDatabase.AssetPathToGUID(p);
            if (!string.IsNullOrEmpty(g)) validGuids.Add(g);
        }

        foreach (string ext in extensions)
        {
            string[] files = System.IO.Directory.GetFiles(
                Application.dataPath, ext, System.IO.SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string content = System.IO.File.ReadAllText(filePath);
                string original = content;

                // MonoBehaviour 블록 단위로 파싱
                // 패턴: MonoBehaviour 블록 안에서 m_Script guid를 찾고
                // 같은 블록 안의 필드 이름들로 스크립트 추론
                var blocks = content.Split(new[] { "--- !u!" },
                    System.StringSplitOptions.None);

                for (int b = 1; b < blocks.Length; b++)
                {
                    string block = blocks[b];
                    if (!block.Contains("MonoBehaviour:")) continue;

                    var guidMatch = System.Text.RegularExpressions.Regex.Match(
                        block, @"m_Script:\s*\{fileID:\s*\d+,\s*guid:\s*([a-f0-9]{32}),\s*type:\s*3\}");
                    if (!guidMatch.Success) continue;

                    string oldGuid = guidMatch.Groups[1].Value;
                    if (validGuids.Contains(oldGuid)) continue;

                    // 이 블록의 m_Name이나 필드 이름에서 힌트 얻기
                    // m_Name은 GameObject 이름이라 안 됨
                    // 대신 m_EditorClassIdentifier 위의 커스텀 필드들로 추론

                    // 방법: 블록 안의 모든 필드 이름 수집
                    var fieldNames = new HashSet<string>();
                    var fieldRegex = new System.Text.RegularExpressions.Regex(
                        @"^\s{2,4}(\w+):", System.Text.RegularExpressions.RegexOptions.Multiline);
                    foreach (System.Text.RegularExpressions.Match fm in fieldRegex.Matches(block))
                    {
                        string field = fm.Groups[1].Value;
                        // Unity 기본 필드 제외
                        if (field.StartsWith("m_") && field != "m_EditorClassIdentifier")
                            continue;
                        if (field == "serializedVersion" || field == "m_Script"
                            || field == "m_Name" || field == "m_EditorHideFlags")
                            continue;
                        fieldNames.Add(field);
                    }

                    // 각 스크립트의 직렬화 필드와 비교해서 가장 일치하는 것 찾기
                    MonoScript bestMatch = null;
                    int bestScore = 0;

                    foreach (var kvp in scriptMap)
                    {
                        var klass = kvp.Value.GetClass();
                        if (klass == null) continue;

                        var classFields = klass.GetFields(
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        int score = 0;
                        foreach (var f in classFields)
                        {
                            if (fieldNames.Contains(f.Name)) score++;
                        }

                        if (score > bestScore && score >= 2)
                        {
                            bestScore = score;
                            bestMatch = kvp.Value;
                        }
                    }

                    if (bestMatch != null)
                    {
                        string newGuid = AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(bestMatch));
                        content = content.Replace(oldGuid, newGuid);
                        fixedCount++;
                        Debug.Log($"교체: {bestMatch.GetClass().Name} " +
                            $"(score:{bestScore}) in {System.IO.Path.GetFileName(filePath)}");
                    }
                    else
                    {
                        // 필드가 없거나 매칭 안 되면 guid만 로깅
                        string fieldInfo = fieldNames.Count > 0
                            ? string.Join(", ", fieldNames.Take(5))
                            : "(필드 없음)";
                        Debug.LogWarning(
                            $"매칭 실패 guid:{oldGuid.Substring(0,8)}... " +
                            $"필드:[{fieldInfo}] in {System.IO.Path.GetFileName(filePath)}");
                        failCount++;
                    }
                }

                if (content != original)
                {
                    System.IO.File.WriteAllText(filePath, content);
                }
            }
        }
    }
}