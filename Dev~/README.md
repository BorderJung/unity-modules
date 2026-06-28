# Dev~ — 모듈 추출 수집본

Drilling 프로젝트에서 재사용 가능한 시스템을 추출하기 위한 **1차 수집 작업 공간**.
폴더명이 `~`로 끝나 Unity가 import 하지 않으므로, 컴파일 영향 없이 파일을 쌓아둘 수 있다.

- **출처**: Drilling (Unity 6000.3.12f1)
- **수집일**: 2026-06-29
- **방식**: `.meta`째 복사하여 **원본 스크립트 GUID 유지** → 프리팹/SO 참조 보존
- **상태**: 일반화·패키지화 **전**. 원본(`Assets/02_Scripts`)은 수정하지 않음.

## 폴더 구조

```
Dev~/
  Settings/
    Scripts/            SettingsSystem 외 9개 (UI 컴포넌트 포함)
    Prefabs/            SettingsSystem, Settings Panel, SettingsBool
    ScriptableObjects/  SettingsSO.asset
    Events/             Settings용 EventChannel 인스턴스 10개
  Localization/
    Scripts/            LocalizationManager, LocalizationTable, LocalizeKeyAttribute, UILocalizeText
    Editor/             LocalizeKeyDrawer, UILocalizeTextEditor
    ScriptableObjects/  LocalizationTable.asset
    Data/               LocalizationImporter.prefab, LocalizationSheetJson.json
  SheetManager/         (프레임워크 + Localization 시트만)
    Scripts/            LocalizationSheetManager, SecretData
    Editor/             LocalizationSheetManagerEditor
    Secret.template.json
  _Shared/              공통 토대
    Log.cs, DescriptionSO.cs
    Events/             Void/Float/Int/Bool/String EventChannelSO
    UI/                 UIGenericButton.cs
    DEPENDENCIES.md     ← 미포함 의존성·경로 하드코딩·정리 메모
```

## 의도적으로 제외한 것

- `Secret.json`(실 Google Sheet URL) — 보안. `SheetManager/Secret.template.json`로 대체.
- `GoogleSheetToolsWindow.cs` — 7개 시트 매니저 전부에 의존.
- Dialog/Upgrade/Alter/Npc/Item/TreasureBox 시트 매니저 — Drilling 도메인 SO에 강결합.
- `Managers.cs` — Drilling 16개 매니저 참조.

## 다음 단계(예정)

1. `namespace BorderJung` 격리 + 전용 asmdef
2. `ISettingsRepository` / `ILocalizationProvider`로 `ProfileSave`/`Managers` 의존 분리
3. `com.borderjung.core` UPM 패키지화 (package.json)
4. 빈/대상 Unity 프로젝트에서 컴파일·참조 무결성 검증

상세는 [`_Shared/DEPENDENCIES.md`](_Shared/DEPENDENCIES.md) 참조.
