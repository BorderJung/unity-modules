# 미포함 의존성 / 이식 시 충족 사항

이 수집물(`Dev~`)은 **원본 GUID를 유지한 1차 수집본**이며 일반화·패키지화 전이다.
대상 프로젝트로 활성화(`Dev~` 밖으로 이동/패키지화)할 때 아래를 충족해야 컴파일된다.

## 1. 필요한 Unity 패키지

| 패키지 | 사용처 | 비고 |
| --- | --- | --- |
| `com.unity.ugui` (TextMeshPro 포함) | `UISettingsDropdown`, `UISettingsSlider`, `UILocalizeText` | TMPro 네임스페이스 |
| `com.unity.nuget.newtonsoft-json` | `LocalizationSheetManager` | `Newtonsoft.Json.Linq` |

## 2. 미포함된 Drilling 고유 의존성 (의도적 제외)

| 클래스 | 참조처 | 처리 방향(일반화 단계) |
| --- | --- | --- |
| `Managers` (정적 레지스트리) | `LocalizationManager`(Services.Localization 등록), `UISettingsGraphicComponent`(언어 라벨 조회) | `ILocalizationProvider` 인터페이스로 대체 |
| `SaveLoadSystem` / `ProfileSave` | `SettingsSystem`, `SettingsSO` (설정 저장/로드) | `ISettingsRepository` 인터페이스로 대체 |
| `AudioManager` | Settings 볼륨/토글 EventChannel 구독측 | 게임이 구독측 구현 (코어는 이벤트 발행만) |

> 위 3개는 Drilling 게임 전반에 결합되어 있어 제외했다. 일반화 단계에서 두 인터페이스
> (`ISettingsRepository`, `ILocalizationProvider`)로 끊는 것이 목표.

## 3. 경로 하드코딩 주의 (`LocalizationSheetManager`)

대상 프로젝트의 폴더 구조가 다르면 인스펙터 `[SerializeField]` 또는 코드에서 조정 필요.

| 용도 | 하드코딩 경로 |
| --- | --- |
| Secret (시트 URL) | `Assets/07_Datas/Secret.json` (※ `Application.dataPath` 기준 고정) |
| 로컬 캐시 JSON | `07_Datas/GenerateGoogleSheet/LocalizationSheetJson.json` |
| 출력 에셋 | `Assets/04_ScriptableObjects/Localization/LocalizationTable.asset` |

- `Secret.json`(실 URL)은 보안상 **수집하지 않았다**. `SheetManager/Secret.template.json`을
  대상 프로젝트의 `Assets/07_Datas/Secret.json`으로 복사하고 URL을 채워 넣을 것.

## 4. 코드 정리 메모 (일반화 단계 처리)

- `_Shared/Events/StringEventChannelSOEventChannelSO.cs` — **파일명 중복 오타**.
  클래스명은 `StringEventChannelSO`. GUID 보존을 위해 현재는 원본 그대로 유지.
  rename 시 `.cs.meta` GUID는 유지하고 파일명만 변경할 것.
- EventChannelSO 5종이 공통 추상 베이스(`EventChannelSO<T>` 등)를 상속하는지 일반화 시 확인.
  별도 베이스 파일이 있으면 추가 수집 필요.
- `Localization/Editor`, `SheetManager/Editor` 스크립트는 `UnityEditor` 의존 →
  패키지화 시 별도 Editor asmdef로 분리.
