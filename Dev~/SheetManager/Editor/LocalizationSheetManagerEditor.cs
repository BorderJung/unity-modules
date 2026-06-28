using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// LocalizationSheetManager의 Fetch 실행 버튼을 제공하는 커스텀 인스펙터이다.
/// </summary>
[CustomEditor(typeof(LocalizationSheetManager))]
public class LocalizationSheetManagerEditor : Editor
{
    /// <summary>
    /// 기본 인스펙터와 Fetch 버튼을 함께 그린다.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);
        if (GUILayout.Button("Fetch Localization Sheet"))
        {
            InvokeFetchLocalizationSheet();
        }
    }

    /// <summary>
    /// Reflection으로 private FetchLocalizationSheet 메서드를 호출한다.
    /// </summary>
    private void InvokeFetchLocalizationSheet()
    {
        LocalizationSheetManager manager = (LocalizationSheetManager)target;
        if (manager == null)
        {
            return;
        }

        MethodInfo method = typeof(LocalizationSheetManager).GetMethod(
            "FetchLocalizationSheet",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (method == null)
        {
            Log.E("[LocalizationSheetManagerEditor] FetchLocalizationSheet 메서드를 찾지 못했습니다.");
            return;
        }

        method.Invoke(manager, null);
    }
}
