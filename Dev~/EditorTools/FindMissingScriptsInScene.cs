using UnityEngine;
using UnityEditor;

public class FindMissingScriptsInScene
{
    [MenuItem("Tools/🔍 Find Missing Scripts In Scene")]
    public static void FindMissingScripts()
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>(true); // 활성/비활성 모두 검색
        int count = 0;

        foreach (GameObject go in gos)
        {
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Log.W($"❗ Missing script in GameObject: '{go.name}'", go);
                    count++;
                }
            }
        }

        Log.D($"🔍 검색 완료: Missing Script {count}개 발견됨");
    }
}