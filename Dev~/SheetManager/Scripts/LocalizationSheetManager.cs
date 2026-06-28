using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Google Sheet JSON 데이터를 가져와 LocalizationTable 에셋을 생성하거나 갱신하는 Importer 컴포넌트이다.
/// </summary>
public class LocalizationSheetManager : MonoBehaviour
{
    [Tooltip("true: google sheet, false: local json")]
    [SerializeField] private bool isAccessLocalizationSheet = true;
    [SerializeField] private string localizationSheetName = "Localization";
    [SerializeField] private string localJsonPath = "07_Datas/GenerateGoogleSheet/LocalizationSheetJson.json";

    [Header("Output")]
    [SerializeField] private string outputAssetPath = "Assets/04_ScriptableObjects/Localization/LocalizationTable.asset";
    [SerializeField] private bool overwriteExistingAsset = true;

    [Header("Validation")]
    [Tooltip("필수 언어 코드. '/' 기준으로 분리. 예: ko/en/ja/zh")]
    [SerializeField] private string requiredLanguageCodes = "ko/en/ja/zh";

    private static readonly string[] KeyAliases = { "KEY", "Key", "key" };
    private static readonly HashSet<string> ReservedColumns = new(StringComparer.OrdinalIgnoreCase) { "KEY", "NOTE" };
    private static readonly Dictionary<string, string> LanguageAliasToCode = new(StringComparer.OrdinalIgnoreCase)
    {
        { "KO", "ko" },
        { "EN", "en" },
        { "JA", "ja" },
        { "ZH", "zh" }
    };

    private string SecretJsonPath => $"{Application.dataPath}/07_Datas/Secret.json";
    private string LocalJsonPath => $"{Application.dataPath}/{localJsonPath}";

#if UNITY_EDITOR
    /// <summary>
    /// 설정값을 기준으로 Localization 시트를 가져와 LocalizationTable 에셋을 생성하거나 갱신한다.
    /// </summary>
    [ContextMenu("Fetch Localization Sheet")]
    private async void FetchLocalizationSheet()
    {
        Log.D($"[LocalizationSheetManager] Fetch 시작 (source: {(isAccessLocalizationSheet ? "GoogleSheet" : "LocalJson")}, sheet: {localizationSheetName})");

        string rawJson = isAccessLocalizationSheet
            ? await LoadLocalizationJsonFromGoogleSheet()
            : LoadLocalizationJsonFromLocal();

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            Log.W("[LocalizationSheetManager] Fetch 중단 - JSON 데이터가 비어 있습니다.");
            return;
        }

        SaveLocalJson(rawJson);

        if (!TryGetRows(rawJson, out JArray rows))
        {
            Log.E($"[LocalizationSheetManager] Fetch 실패 - 시트 '{localizationSheetName}'를 찾지 못했거나 형식이 올바르지 않습니다.");
            return;
        }

        string[] requiredCodes = ParseRequiredLanguageCodes(requiredLanguageCodes);
        bool hasError = BuildEntries(rows, requiredCodes, out List<LocalizationEntry> entries);
        if (hasError)
        {
            Log.W("[LocalizationSheetManager] 검증 오류가 있어 에셋 갱신을 중단합니다.");
            return;
        }

        SaveLocalizationTableAsset(entries);
    }

    /// <summary>
    /// Secret JSON에 저장된 URL로 Google Sheet JSON 원문을 다운로드한다.
    /// </summary>
    /// <returns>다운로드한 JSON 문자열 또는 실패 시 null</returns>
    private async Task<string> LoadLocalizationJsonFromGoogleSheet()
    {
        if (!File.Exists(SecretJsonPath))
        {
            Log.E($"[LocalizationSheetManager] Secret 파일을 찾을 수 없습니다: {SecretJsonPath}");
            return null;
        }

        string secretJson = File.ReadAllText(SecretJsonPath, Encoding.UTF8);
        SecretData secret = JsonUtility.FromJson<SecretData>(secretJson);
        if (secret == null || string.IsNullOrWhiteSpace(secret.googleSheetUrl))
        {
            Log.E("[LocalizationSheetManager] Secret 데이터에 googleSheetUrl이 없습니다.");
            return null;
        }

        using HttpClient client = new HttpClient();
        try
        {
            byte[] bytes = await client.GetByteArrayAsync(secret.googleSheetUrl);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception exception)
        {
            Log.E($"[LocalizationSheetManager] 다운로드 실패: {exception.Message}");
            return null;
        }
    }

    /// <summary>
    /// 로컬 캐시 JSON 파일에서 Localization 원문을 읽어온다.
    /// </summary>
    /// <returns>로컬 JSON 문자열 또는 파일이 없으면 null</returns>
    private string LoadLocalizationJsonFromLocal()
    {
        if (!File.Exists(LocalJsonPath))
        {
            Log.W($"[LocalizationSheetManager] 로컬 JSON 파일이 없습니다: {LocalJsonPath}");
            return null;
        }

        return File.ReadAllText(LocalJsonPath, Encoding.UTF8);
    }

    /// <summary>
    /// 가져온 JSON 원문을 로컬 캐시 경로에 UTF-8로 저장한다.
    /// </summary>
    /// <param name="rawJson">저장할 JSON 원문</param>
    private void SaveLocalJson(string rawJson)
    {
        string directory = Path.GetDirectoryName(LocalJsonPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(LocalJsonPath, rawJson, new UTF8Encoding(false));
    }

    /// <summary>
    /// 원문 JSON에서 대상 시트의 row 배열을 찾아 반환한다.
    /// </summary>
    /// <param name="rawJson">Google Sheet JSON 원문</param>
    /// <param name="rows">파싱된 row 배열</param>
    /// <returns>row 배열 획득 성공 여부</returns>
    private bool TryGetRows(string rawJson, out JArray rows)
    {
        rows = null;

        JToken root = JToken.Parse(rawJson);
        if (root is JArray rootArray)
        {
            rows = rootArray;
            return rows.Count > 0;
        }

        if (root is not JObject rootObject)
        {
            return false;
        }

        foreach (JProperty property in rootObject.Properties())
        {
            if (!string.Equals(property.Name, localizationSheetName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value is JArray sheetRows)
            {
                rows = sheetRows;
                return rows.Count > 0;
            }
        }

        return false;
    }

    /// <summary>
    /// row 배열을 LocalizationEntry 목록으로 변환하고 검증 오류 여부를 함께 반환한다.
    /// </summary>
    /// <param name="rows">시트 row 배열</param>
    /// <param name="requiredCodes">필수 언어 코드 목록</param>
    /// <param name="entries">변환된 엔트리 목록</param>
    /// <returns>검증 오류가 하나라도 있으면 true</returns>
    private bool BuildEntries(JArray rows, string[] requiredCodes, out List<LocalizationEntry> entries)
    {
        entries = new List<LocalizationEntry>();
        bool hasError = false;
        HashSet<string> seenKeys = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] is not JObject row)
            {
                continue;
            }

            string key = ReadKey(row);
            if (string.IsNullOrWhiteSpace(key))
            {
                Log.W($"[LocalizationSheetManager] row {i + 1}: KEY가 비어 있어 건너뜁니다.");
                continue;
            }

            if (!seenKeys.Add(key))
            {
                hasError = true;
                Log.E($"[LocalizationSheetManager] 중복 KEY: {key}");
                continue;
            }

            Dictionary<string, string> translationMap = ReadTranslations(row);
            hasError |= ValidateRequiredTranslations(key, translationMap, requiredCodes);

            LocalizationEntry entry = new LocalizationEntry
            {
                key = key,
                translations = ToPairs(translationMap)
            };

            entries.Add(entry);
        }

        entries.Sort((left, right) => string.CompareOrdinal(left.key, right.key));
        Log.D($"[LocalizationSheetManager] 파싱 완료 - entries: {entries.Count}");
        return hasError;
    }

    /// <summary>
    /// row 객체에서 KEY 컬럼 값을 읽어 반환한다.
    /// </summary>
    /// <param name="row">시트의 단일 row 객체</param>
    /// <returns>KEY 문자열</returns>
    private string ReadKey(JObject row)
    {
        for (int i = 0; i < KeyAliases.Length; i++)
        {
            string alias = KeyAliases[i];
            foreach (JProperty property in row.Properties())
            {
                if (!string.Equals(property.Name, alias, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return property.Value?.ToString()?.Trim();
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// row 객체의 언어 컬럼을 언어 코드 기반 Dictionary로 변환한다.
    /// </summary>
    /// <param name="row">시트의 단일 row 객체</param>
    /// <returns>언어 코드별 번역 문자열 Dictionary</returns>
    private Dictionary<string, string> ReadTranslations(JObject row)
    {
        Dictionary<string, string> translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (JProperty property in row.Properties())
        {
            if (ReservedColumns.Contains(property.Name))
            {
                continue;
            }

            string languageCode = NormalizeLanguageCode(property.Name);
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                continue;
            }

            if (translations.ContainsKey(languageCode))
            {
                Log.W($"[LocalizationSheetManager] 중복 언어 컬럼 감지: {property.Name} -> {languageCode}");
                continue;
            }

            translations.Add(languageCode, property.Value?.ToString() ?? string.Empty);
        }

        return translations;
    }

    /// <summary>
    /// 필수 언어 코드에 대한 번역 누락과 빈 문자열 여부를 검증한다.
    /// </summary>
    /// <param name="key">검증 대상 로컬라이징 키</param>
    /// <param name="translations">언어 코드별 번역 Dictionary</param>
    /// <param name="requiredCodes">필수 언어 코드 목록</param>
    /// <returns>검증 오류가 있으면 true</returns>
    private bool ValidateRequiredTranslations(string key, Dictionary<string, string> translations, string[] requiredCodes)
    {
        bool hasError = false;

        for (int i = 0; i < requiredCodes.Length; i++)
        {
            string requiredCode = requiredCodes[i];
            if (!translations.TryGetValue(requiredCode, out string value))
            {
                hasError = true;
                Log.E($"[LocalizationSheetManager] ERROR: {key} missing {requiredCode.ToUpperInvariant()} translation");
                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                hasError = true;
                Log.E($"[LocalizationSheetManager] ERROR: {key} has empty {requiredCode.ToUpperInvariant()} translation");
            }
        }

        return hasError;
    }

    /// <summary>
    /// 언어 코드 Dictionary를 직렬화 가능한 LocalizedTextPair 목록으로 변환한다.
    /// </summary>
    /// <param name="translationMap">언어 코드별 번역 문자열 Dictionary</param>
    /// <returns>직렬화 가능한 번역 쌍 목록</returns>
    private List<LocalizedTextPair> ToPairs(Dictionary<string, string> translationMap)
    {
        List<LocalizedTextPair> pairs = new List<LocalizedTextPair>(translationMap.Count);
        foreach (KeyValuePair<string, string> pair in translationMap)
        {
            pairs.Add(new LocalizedTextPair
            {
                languageCode = pair.Key,
                value = pair.Value
            });
        }

        pairs.Sort((left, right) => string.CompareOrdinal(left.languageCode, right.languageCode));
        return pairs;
    }

    /// <summary>
    /// 입력 컬럼명을 프로젝트 표준 언어 코드로 정규화한다.
    /// </summary>
    /// <param name="columnName">시트 컬럼명</param>
    /// <returns>정규화된 소문자 언어 코드</returns>
    private string NormalizeLanguageCode(string columnName)
    {
        if (LanguageAliasToCode.TryGetValue(columnName, out string knownCode))
        {
            return knownCode;
        }

        return columnName.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 구분자('/') 기반 문자열을 필수 언어 코드 배열로 파싱한다.
    /// </summary>
    /// <param name="rawCodes">직렬화된 필수 언어 코드 문자열</param>
    /// <returns>정규화된 필수 언어 코드 배열</returns>
    private string[] ParseRequiredLanguageCodes(string rawCodes)
    {
        if (string.IsNullOrWhiteSpace(rawCodes))
        {
            return Array.Empty<string>();
        }

        string[] splitCodes = rawCodes.Split('/');
        List<string> normalized = new List<string>(splitCodes.Length);

        for (int i = 0; i < splitCodes.Length; i++)
        {
            string code = splitCodes[i].Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            if (!normalized.Contains(code))
            {
                normalized.Add(code);
            }
        }

        return normalized.ToArray();
    }

    /// <summary>
    /// LocalizationTable 에셋을 생성하거나 갱신한다.
    /// </summary>
    /// <param name="entries">저장할 로컬라이징 엔트리 목록</param>
    private void SaveLocalizationTableAsset(List<LocalizationEntry> entries)
    {
        EnsureOutputFolder(outputAssetPath);

        LocalizationTable table = AssetDatabase.LoadAssetAtPath<LocalizationTable>(outputAssetPath);
        bool created = false;

        if (table == null)
        {
            table = ScriptableObject.CreateInstance<LocalizationTable>();
            table.SetEntries(entries);
            AssetDatabase.CreateAsset(table, outputAssetPath);
            created = true;
        }
        else
        {
            if (!overwriteExistingAsset)
            {
                Log.W("[LocalizationSheetManager] 기존 에셋이 존재하여 갱신을 건너뜁니다.");
                return;
            }

            table.SetEntries(entries);
            EditorUtility.SetDirty(table);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log.D($"[LocalizationSheetManager] Fetch 완료 (entries: {entries.Count}, created: {created}, path: {outputAssetPath})");
    }

    /// <summary>
    /// 출력 에셋 경로의 상위 폴더가 없으면 순차적으로 생성한다.
    /// </summary>
    /// <param name="assetPath">생성 대상 에셋 경로</param>
    private void EnsureOutputFolder(string assetPath)
    {
        string folderPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        if (parts.Length == 0)
        {
            return;
        }

        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
#endif
}
