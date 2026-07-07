# Changelog

All notable changes to `com.borderjung.unity-modules` are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/) and this project adheres to [SemVer](https://semver.org/).

## [1.2.0] - 2026-07-07

### Added
- **Input** (`Border.Input`): `InputReader` ScriptableObject — Input System 콜백을 `UnityAction` 이벤트(`MoveEvent`/`JumpEvent`/`MenuPauseEvent`/`MenuCloseEvent`)로 브리지. 자체 asmdef `Border.Input`으로 격리하고 `versionDefines`/`defineConstraints`(`BORDER_INPUTSYSTEM`)로 게이팅 — `com.unity.inputsystem`이 설치된 프로젝트에서만 컴파일되며 패키지 차원의 강제 의존성은 없음(§5 optional).

### Note
- `InputReader`는 `.inputactions`에서 생성되는 `GameInput` 클래스에 의존합니다. 이 생성 코드/에셋은 아직 패키지에 포함되지 않아, 소비 프로젝트가 해당 `.inputactions`(+ 생성된 `GameInput`)를 제공하기 전까지 `Border.Input` 모듈은 컴파일되지 않습니다. 전용 asmdef 격리 덕분에 `Border` 본체 어셈블리에는 영향이 없습니다.

## [1.1.1] - 2026-06-29

### Fixed
- Compile errors from the v1.1.0 fold (v1.1.0 did not compile):
  - `UIGenericButton` (`Border.UI`) was missing `using Border.Localization` for its `UILocalizeText` reference (CS0246).
  - `UILocalizeTextEditor`'s base class `Editor` resolved to the `Border.Localization.Editor` namespace → qualified as `UnityEditor.Editor` (CS0118).

### Added
- `Samples~/SettingsAndLocalization` — example `SettingsSO`, `LocalizationTable`, event-channel instances (script GUIDs rewired from `_Shared` to `Border.Events`), and Settings panel prefabs. Registered via `package.json` `samples`.

## [1.1.0] - 2026-06-29

### Added
- **SaveLoad** (`Border.SaveLoad`): JSON save/load — `SaveLoadSystem`, `FileManager`, `Save`.
- **Localization** (`Border.Localization`): SO-based table + `LocalizationManager` (implements `ILocalizationProvider`, static `Current` self-registration), `UILocalizeText`, `[LocalizeKey]` + custom editors (in the new `Border.Editor` assembly).
- **Settings** (`Border.Settings`): SO-driven settings (`SettingsSystem`/`SettingsSO`) with `ISettingsRepository` injection, plus graphics/audio UI components.
- **UI** (`Border.UI`): `UIGenericButton`.

### Changed
- Decoupled from source-game systems: `Managers.Services.Localization` → `LocalizationManager.Current`; settings persistence (`SaveLoadSystem`/`ProfileSave`) → `ISettingsRepository`.
- Added dependency on `com.unity.ugui` (TextMeshPro/UGUI). Editor code split into the `Border.Editor` assembly.

### Not yet included
- **Input** (`InputReader`) — depends on a generated `GameInput` class (`.inputactions`) that was not collected; staged in `Dev~/Input` until that asset is brought in.

## [1.0.0] - 2026-06-29

### Added
- Initial release as a single, root-level UPM package importable by bare git URL (no `?path=`).
- **Core** (`Border.Core`): build-stripped conditional `Log`, deterministic xorshift32 `DeterministicRng`, `ScreenshotManager`.
- **Events** (`Border.Events`): ScriptableObject event channels — Void/Bool/Int/Float/Vector2/String, plus Fade and FloatingHud channels.
- Single assembly `Border`, zero third-party dependencies.
