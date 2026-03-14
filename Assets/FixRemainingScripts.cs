using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class FixRemainingScripts
{
    // 수동 매핑: 옛 guid -> 새 클래스 이름
    static readonly Dictionary<string, string> manualMap = new Dictionary<string, string>
    {
        { "93dd7e5fa9affde4e8506453a7a2cfe3", "GameManager" },
        { "be870cf6ee22b2e4fac1b4b5577abe33", "GameScene" },
        { "54a31d99c35fc674bb1da98b665b9306", "CheatManager" },
    };

    [MenuItem("Tools/Fix Remaining Scripts")]
    static void Fix()
    {
        // 클래스 이름 -> 새 guid 매핑
        var nameToGuid = new Dictionary<string, string>();
        foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
        {
            var klass = ms.GetClass();
            if (klass == null) continue;
            string path = AssetDatabase.GetAssetPath(ms);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
                nameToGuid[klass.Name] = guid;
        }

        // 매핑 테이블 만들기: 옛 guid -> 새 guid
        var guidReplace = new Dictionary<string, string>();
        foreach (var kvp in manualMap)
        {
            if (nameToGuid.TryGetValue(kvp.Value, out string newGuid))
            {
                guidReplace[kvp.Key] = newGuid;
                Debug.Log($"매핑: {kvp.Value} ({kvp.Key.Substring(0,8)}... -> {newGuid.Substring(0,8)}...)");
            }
            else
            {
                Debug.LogWarning($"클래스 '{kvp.Value}'를 찾을 수 없음!");
            }
        }

        // 파일 스캔 및 교체
        string[] extensions = new[] { "*.prefab", "*.unity", "*.asset" };
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

                foreach (var kvp in guidReplace)
                {
                    if (content.Contains(kvp.Key))
                    {
                        int count = Regex.Matches(content, kvp.Key).Count;
                        content = content.Replace(kvp.Key, kvp.Value);
                        fixedRefs += count;
                        Debug.Log($"교체 {count}건: {manualMap[kvp.Key]} in {Path.GetFileName(filePath)}");
                    }
                }

                if (content != original)
                {
                    File.WriteAllText(filePath, content);
                    fixedFiles++;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"=== 완료! {fixedFiles}개 파일에서 {fixedRefs}개 참조 수정 ===");
    }
}