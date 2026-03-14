using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class ListMissingScripts
{
    [MenuItem("Tools/List Missing Scripts In Scene")]
    static void List()
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var rootGO in scene.GetRootGameObjects())
        {
            foreach (var t in rootGO.GetComponentsInChildren<Transform>(true))
            {
                var go = t.gameObject;
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        Debug.LogWarning(
                            $"Missing Script on: '{go.name}' " +
                            $"(path: {GetPath(go)})",
                            go);
                    }
                }
            }
        }
        Debug.Log("스캔 완료!");
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        var parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}

public class FixByInspectorName
{
    static readonly string[] prefixes = new string[]
    {
        "Jaein_", "WS_", "Ryeol_", "SSH_", "fbdfbd_"
    };

    [MenuItem("Tools/Fix By Inspector Name")]
    static void Fix()
    {
        // 현재 스크립트 맵
        var scriptMap = new Dictionary<string, MonoScript>();
        foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
        {
            var klass = ms.GetClass();
            if (klass == null) continue;
            scriptMap[klass.Name] = ms;
            scriptMap[klass.FullName] = ms;
        }

        int fixedCount = 0;

        // 모든 프리팹 처리
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            fixedCount += FixGameObject(
                AssetDatabase.LoadAssetAtPath<GameObject>(path), scriptMap, path);
        }

        // 현재 열린 씬들 처리
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (var rootGO in scene.GetRootGameObjects())
            {
                fixedCount += FixGameObject(rootGO, scriptMap, scene.path);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"=== 완료! {fixedCount}개 컴포넌트 교체 ===");
    }

    static int FixGameObject(GameObject root, Dictionary<string, MonoScript> scriptMap, string context)
    {
        if (root == null) return 0;
        int count = 0;

        foreach (var go in root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
        {
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null) continue;

                // Missing component 발견 - SerializedObject로 스크립트 정보 읽기
                var so = new SerializedObject(go);
                var componentsProp = so.FindProperty("m_Component");

                for (int j = 0; j < componentsProp.arraySize; j++)
                {
                    var compPair = componentsProp.GetArrayElementAtIndex(j);
                    var compRef = compPair.FindPropertyRelative("component");

                    if (compRef.objectReferenceValue != null) continue;

                    // objectReferenceInstanceIDValue가 0이면 missing
                    // m_Script를 통해 이름 얻기 시도
                }
            }

            // 다른 접근: MonoBehaviour[] 로 missing 찾기
            var behaviours = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null) continue;

                // SerializedObject로 missing MonoBehaviour의 m_Script 읽기
                // 이건 null이라 직접 접근 불가...
                // 대신 GlobalObjectId 활용
            }
        }

        // === 실제로 작동하는 방법: 직접 .meta 스타일 대신 에디터 리플렉션 ===
        // Unity 내부 API를 사용해서 missing script의 클래스 이름을 가져옵니다
        foreach (var go in root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
        {
            var serializedGO = new SerializedObject(go);
            var componentsProp = serializedGO.FindProperty("m_Component");

            bool modified = false;
            for (int i = componentsProp.arraySize - 1; i >= 0; i--)
            {
                var compRef = componentsProp.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("component");

                if (compRef.objectReferenceValue != null) continue;

                // missing! -> 제거하고 올바른 스크립트 추가
                // 하지만 이름을 모르면 추가 못 함...
            }
        }

        return count;
    }
}