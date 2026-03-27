#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

public class FixAll
{
    static readonly string[] prefixes = new string[]
    {
        "Jaein_", "WS_", "Ryeol_", "SSH_", "fbdfbd_"
    };

    [MenuItem("Tools/[FINAL] Fix ALL Missing Scripts")]
    static void Fix()
    {
        // ======================================================
        // STEP 1: 현재 존재하는 모든 스크립트의 매핑 테이블 구축
        // ======================================================
        // className -> MonoScript guid
        var nameToGuid = new Dictionary<string, string>();
        // className -> MonoScript (for field matching)
        var nameToScript = new Dictionary<string, MonoScript>();
        // Set of valid guids
        var validGuids = new HashSet<string>();

        foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
        {
            var klass = ms.GetClass();
            if (klass == null) continue;

            string path = AssetDatabase.GetAssetPath(ms);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) continue;

            nameToGuid[klass.Name] = guid;
            nameToScript[klass.Name] = ms;
            validGuids.Add(guid);
        }

        // ======================================================
        // STEP 2: prefix 기반 역매핑 테이블 구축
        //   "Jaein_WeaponChargeInfo" -> "WeaponChargeInfo" 의 guid
        // ======================================================
        var oldNameToNewGuid = new Dictionary<string, string>();
        foreach (var kvp in nameToGuid)
        {
            string className = kvp.Key;
            string newGuid = kvp.Value;

            foreach (string prefix in prefixes)
            {
                string oldName = prefix + className;
                oldNameToNewGuid[oldName] = newGuid;
            }
            // 또한 이름 자체도 (prefix 없는 버전)
            oldNameToNewGuid[className] = newGuid;
        }

        // ======================================================
        // STEP 3: YAML 파싱으로 missing guid 수집 + 컨텍스트 정보
        // ======================================================
        string[] extensions = new[] { "*.prefab", "*.unity", "*.asset" };
        
        // missing guid -> 관련 정보
        var missingGuidInfo = new Dictionary<string, MissingInfo>();
        // 파일별 교체 계획
        var replacePlan = new Dictionary<string, string>(); // oldGuid -> newGuid

        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(
                Application.dataPath, ext, SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string content = File.ReadAllText(filePath);
                AnalyzeYaml(content, filePath, validGuids, nameToScript, 
                    nameToGuid, missingGuidInfo);
            }
        }

        Debug.Log($"고유 missing GUID: {missingGuidInfo.Count}개 발견");

        // ======================================================
        // STEP 4: 각 missing guid에 대해 최적 매칭 찾기
        // ======================================================
        foreach (var kvp in missingGuidInfo)
        {
            string oldGuid = kvp.Key;
            MissingInfo info = kvp.Value;

            string newGuid = null;
            string matchReason = "";

            // 방법 1: GO 이름 기반 매칭
            if (newGuid == null && info.goNames.Count > 0)
            {
                foreach (string goName in info.goNames)
                {
                    // GO 이름에서 직접 매칭 시도
                    // 예: "@EnemySpawner" -> "EnemySpawner"
                    string cleanName = goName.TrimStart('@', '[', ']', ' ');
                    cleanName = cleanName.Split(' ')[0]; // "Root Ring" -> "Root"

                    // 1a: 정확한 매칭
                    if (nameToGuid.TryGetValue(cleanName, out string g))
                    {
                        newGuid = g;
                        matchReason = $"GO이름 정확매칭: {goName} -> {cleanName}";
                        break;
                    }

                    // 1b: prefix + GO이름으로 현재 스크립트 찾기
                    foreach (string prefix in prefixes)
                    {
                        string withPrefix = prefix + cleanName;
                        if (nameToGuid.TryGetValue(withPrefix, out string g2))
                        {
                            newGuid = g2;
                            matchReason = $"GO이름+prefix매칭: {goName} -> {withPrefix}";
                            break;
                        }
                    }
                    if (newGuid != null) break;

                    // 1c: GO이름이 old name(prefix포함)인 경우
                    if (oldNameToNewGuid.TryGetValue(cleanName, out string g3))
                    {
                        newGuid = g3;
                        matchReason = $"GO이름이 oldName: {goName} -> {cleanName}";
                        break;
                    }
                }
            }

            // 방법 2: 필드 기반 매칭 (threshold 1)
            if (newGuid == null && info.fieldNames.Count > 0)
            {
                var customFields = info.fieldNames
                    .Where(f => f != "m_EditorClassIdentifier")
                    .ToHashSet();

                if (customFields.Count > 0)
                {
                    MonoScript bestMatch = null;
                    int bestScore = 0;
                    string bestName = "";

                    foreach (var scriptKvp in nameToScript)
                    {
                        var klass = scriptKvp.Value.GetClass();
                        if (klass == null) continue;

                        var classFields = klass.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | 
                            BindingFlags.Instance);

                        int score = 0;
                        foreach (var f in classFields)
                        {
                            if (customFields.Contains(f.Name)) score++;
                        }

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMatch = scriptKvp.Value;
                            bestName = scriptKvp.Key;
                        }
                    }

                    if (bestMatch != null && bestScore >= 1)
                    {
                        newGuid = AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(bestMatch));
                        matchReason = $"필드매칭(score:{bestScore}): {bestName} " +
                            $"fields:[{string.Join(",", customFields.Take(3))}]";
                    }
                }
            }

            // 방법 3: Library 캐시에서 class name 검색
            if (newGuid == null)
            {
                string cachedName = SearchLibraryForGuid(oldGuid);
                if (!string.IsNullOrEmpty(cachedName))
                {
                    // cachedName은 "Assembly-CSharp::Jaein_WeaponChargeInfo" 같은 형태
                    string className = cachedName;
                    if (className.Contains("::"))
                        className = className.Split(new[] { "::" }, 
                            System.StringSplitOptions.None).Last();

                    // prefix 제거 시도
                    string stripped = className;
                    foreach (string prefix in prefixes)
                    {
                        if (className.StartsWith(prefix))
                        {
                            stripped = className.Substring(prefix.Length);
                            break;
                        }
                    }
                    // 첫 번째 _ 기준 시도
                    if (stripped == className && className.Contains("_"))
                    {
                        int idx = className.IndexOf('_');
                        stripped = className.Substring(idx + 1);
                    }

                    // 매칭 시도: stripped 이름 또는 원래 이름
                    if (nameToGuid.TryGetValue(stripped, out string g))
                    {
                        newGuid = g;
                        matchReason = $"Library캐시: {cachedName} -> {stripped}";
                    }
                    else if (nameToGuid.TryGetValue(className, out string g2))
                    {
                        newGuid = g2;
                        matchReason = $"Library캐시(원래이름): {cachedName}";
                    }
                }
            }

            if (newGuid != null)
            {
                replacePlan[oldGuid] = newGuid;
                info.matchResult = matchReason;
            }
        }

        // ======================================================
        // STEP 5: 실제 파일 교체
        // ======================================================
        int fixedFiles = 0;
        int fixedRefs = 0;

        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(
                Application.dataPath, ext, SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string content = File.ReadAllText(filePath);
                string original = content;

                foreach (var kvp in replacePlan)
                {
                    if (content.Contains(kvp.Key))
                    {
                        int count = Regex.Matches(content, kvp.Key).Count;
                        content = content.Replace(kvp.Key, kvp.Value);
                        fixedRefs += count;
                    }
                }

                if (content != original)
                {
                    File.WriteAllText(filePath, content);
                    fixedFiles++;
                }
            }
        }

        // ======================================================
        // STEP 6: 결과 리포트
        // ======================================================
        Debug.Log("========== 교체 성공 목록 ==========");
        foreach (var kvp in missingGuidInfo.Where(x => x.Value.matchResult != null))
        {
            Debug.Log($"  {kvp.Key.Substring(0,8)}... => {kvp.Value.matchResult}");
        }

        var failed = missingGuidInfo.Where(x => x.Value.matchResult == null).ToList();
        if (failed.Count > 0)
        {
            Debug.LogWarning($"========== 매칭 실패: {failed.Count}개 ==========");
            foreach (var kvp in failed)
            {
                var info = kvp.Value;
                string goInfo = info.goNames.Count > 0 
                    ? string.Join(", ", info.goNames.Take(3)) 
                    : "(GO이름 없음)";
                string fieldInfo = info.fieldNames.Count > 0
                    ? string.Join(", ", info.fieldNames
                        .Where(f => f != "m_EditorClassIdentifier").Take(5))
                    : "(필드 없음)";
                string fileInfo = info.foundInFiles.Count > 0
                    ? string.Join(", ", info.foundInFiles.Take(2)
                        .Select(f => Path.GetFileName(f)))
                    : "";

                Debug.LogWarning(
                    $"  guid:{kvp.Key.Substring(0,12)}... " +
                    $"GO:[{goInfo}] 필드:[{fieldInfo}] " +
                    $"파일:[{fileInfo}]");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"\n=== 최종 결과: {fixedFiles}개 파일, {fixedRefs}개 참조 수정, " +
            $"{failed.Count}개 매칭 실패 ===");
    }

    // ======================================================
    // YAML 분석: MonoBehaviour 블록에서 정보 추출
    // ======================================================
    static void AnalyzeYaml(string content, string filePath,
        HashSet<string> validGuids,
        Dictionary<string, MonoScript> nameToScript,
        Dictionary<string, string> nameToGuid,
        Dictionary<string, MissingInfo> missingGuidInfo)
    {
        // 먼저 모든 GameObject 이름 매핑 구축 (fileID -> name)
        var goNames = new Dictionary<string, string>();
        var goBlockRegex = new Regex(
            @"--- !u!1 &(\d+)\s+GameObject:.*?m_Name:\s*(.+?)$",
            RegexOptions.Multiline | RegexOptions.Singleline);

        // 간단한 파싱: --- !u!1 &ID 블록에서 m_Name 추출
        string[] sections = content.Split(new[] { "--- !u!" }, 
            System.StringSplitOptions.None);

        foreach (string section in sections)
        {
            if (section.StartsWith("1 &"))
            {
                // GameObject 블록
                var idMatch = Regex.Match(section, @"^1 &(\d+)");
                var nameMatch = Regex.Match(section, @"m_Name:\s*(.+)$", 
                    RegexOptions.Multiline);
                if (idMatch.Success && nameMatch.Success)
                {
                    goNames[idMatch.Groups[1].Value] = nameMatch.Groups[1].Value.Trim();
                }
            }
        }

        // MonoBehaviour 블록 분석
        foreach (string section in sections)
        {
            if (!section.Contains("MonoBehaviour:")) continue;

            // m_Script guid 추출
            var guidMatch = Regex.Match(section,
                @"m_Script:\s*\{fileID:\s*\d+,\s*guid:\s*([a-f0-9]{32}),\s*type:\s*3\}");
            if (!guidMatch.Success) continue;

            string scriptGuid = guidMatch.Groups[1].Value;
            if (validGuids.Contains(scriptGuid)) continue; // 정상이면 스킵

            // Missing 발견!
            if (!missingGuidInfo.ContainsKey(scriptGuid))
            {
                missingGuidInfo[scriptGuid] = new MissingInfo();
            }
            var info = missingGuidInfo[scriptGuid];
            info.foundInFiles.Add(filePath);

            // m_GameObject fileID 추출 -> GO 이름 찾기
            var goRefMatch = Regex.Match(section, 
                @"m_GameObject:\s*\{fileID:\s*(\d+)");
            if (goRefMatch.Success)
            {
                string goFileId = goRefMatch.Groups[1].Value;
                if (goNames.TryGetValue(goFileId, out string goName))
                {
                    info.goNames.Add(goName);
                }
            }

            // 커스텀 필드 추출
            var fieldRegex = new Regex(
                @"^\s{2,4}(\w+):", RegexOptions.Multiline);
            foreach (Match fm in fieldRegex.Matches(section))
            {
                string field = fm.Groups[1].Value;
                if (field.StartsWith("m_") && field != "m_EditorClassIdentifier")
                    continue;
                if (field == "serializedVersion" || field == "m_Script" 
                    || field == "m_Name" || field == "m_EditorHideFlags"
                    || field == "m_ObjectHideFlags" || field == "m_CorrespondingSourceObject"
                    || field == "m_PrefabInstance" || field == "m_PrefabAsset"
                    || field == "m_GameObject" || field == "m_Enabled")
                    continue;
                info.fieldNames.Add(field);
            }
        }
    }

    // ======================================================
    // Library 폴더에서 GUID -> class name 캐시 검색
    // ======================================================
    static string SearchLibraryForGuid(string guid)
    {
        string libraryPath = Path.Combine(Application.dataPath, "..", "Library");

        // 방법 1: MonoManager.asset 파일 검색 (GUID -> class mapping)
        string monoManagerPath = Path.Combine(libraryPath, "MonoManager.asset");
        if (File.Exists(monoManagerPath))
        {
            try
            {
                string monoContent = File.ReadAllText(monoManagerPath);
                if (monoContent.Contains(guid))
                {
                    // GUID 근처에서 class name 추출 시도
                    int idx = monoContent.IndexOf(guid);
                    if (idx >= 0)
                    {
                        // 주변 텍스트에서 class name 패턴 찾기
                        int start = System.Math.Max(0, idx - 200);
                        int end = System.Math.Min(monoContent.Length, idx + 200);
                        string context = monoContent.Substring(start, end - start);

                        var classMatch = Regex.Match(context, 
                            @"([A-Za-z_]\w*::[A-Za-z_]\w+)");
                        if (classMatch.Success)
                            return classMatch.Groups[1].Value;
                    }
                }
            }
            catch { }
        }

        // 방법 2: ScriptRef 폴더 검색
        string scriptRefPath = Path.Combine(libraryPath, "ScriptRef");
        if (Directory.Exists(scriptRefPath))
        {
            try
            {
                foreach (string file in Directory.GetFiles(scriptRefPath, "*", 
                    SearchOption.AllDirectories))
                {
                    string fileContent = File.ReadAllText(file);
                    if (fileContent.Contains(guid))
                    {
                        var classMatch = Regex.Match(fileContent,
                            guid + @"[^}]*?""className""\s*:\s*""([^""]+)""");
                        if (classMatch.Success)
                            return classMatch.Groups[1].Value;
                    }
                }
            }
            catch { }
        }

        // 방법 3: 아무 파일에서나 GUID 근처 class name 검색
        string[] cacheFiles = new[]
        {
            "CurrentLayout-default.dwlt",
            "InspectorExpandedItems.asset",
        };
        foreach (string cf in cacheFiles)
        {
            string path = Path.Combine(libraryPath, cf);
            if (!File.Exists(path)) continue;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string text = System.Text.Encoding.UTF8.GetString(bytes);
                if (text.Contains(guid))
                {
                    int idx = text.IndexOf(guid);
                    int start = System.Math.Max(0, idx - 300);
                    int end = System.Math.Min(text.Length, idx + 300);
                    string ctx = text.Substring(start, end - start);

                    // Assembly-CSharp::ClassName 패턴
                    var cm = Regex.Match(ctx, @"Assembly-CSharp::(\w+)");
                    if (cm.Success) return cm.Groups[1].Value;

                    // 그냥 _ClassName 패턴
                    foreach (string prefix in prefixes)
                    {
                        var pm = Regex.Match(ctx, prefix + @"(\w+)");
                        if (pm.Success) return prefix + pm.Groups[1].Value;
                    }
                }
            }
            catch { }
        }

        return null;
    }

    class MissingInfo
    {
        public HashSet<string> goNames = new HashSet<string>();
        public HashSet<string> fieldNames = new HashSet<string>();
        public List<string> foundInFiles = new List<string>();
        public string matchResult = null;
    }
}
#endif