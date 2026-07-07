# Changelog

All notable changes to `com.borderjung.unity-modules` are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/) and this project adheres to [SemVer](https://semver.org/).

## [1.4.4] - 2026-07-08

### Fixed
- `InputReader` 컴파일 에러 수정.

## [1.4.3] - 2026-07-08

### Changed
- `InputReader` 정리.

## [1.4.2] - 2026-07-08

### Changed
- 샘플을 통합 단일 항목으로 되돌림 — `path`를 `Plugins~/borderjung`로 두어 Localization/Settings/UI를 **Import 한 번에** 가져오도록. (교차 참조되는 `Frame.prefab` 포함 안전)

## [1.4.1] - 2026-07-07

### Changed
- 샘플 에셋 위치를 `Samples~/{Localization,Settings,UI}` → `Plugins~/borderjung/{Localization,Settings,UI}`로 이동, `package.json` samples `path` 갱신. (동작 동일 — Package Manager Import 버튼으로 개별 임포트)

## [1.4.0] - 2026-07-07

### Added
- **Input** (`Border.Input`): 생성 클래스 `GameInput`을 편입해 모듈이 실제 컴파일·동작. Drilling의 `GameInput.cs`를 `namespace Border.Input`으로 감싸 전역 `GameInput` 충돌 회피(소비 프로젝트가 자체 `GameInput`을 가져도 무방). 이제 `com.unity.inputsystem` 설치 프로젝트에서 `Border.Input`이 정상 컴파일된다.

### Fixed
- `Border.Input` 미컴파일로 프로젝트 전체 컴파일이 실패하던 문제 해소 → 임포트한 샘플 프리팹의 스크립트 연결(“missing script”)도 함께 정상화.

## [1.3.2] - 2026-07-07

### Changed
- 샘플을 폴더별 독립 3개 항목으로 노출 — `Localization` / `Settings` / `UI` 각각 Package Manager에서 개별 Import. (래퍼 폴더 제거, `Samples~` 루트 단일 경로 대신 3개 `path`)

## [1.3.1] - 2026-07-07

### Changed
- 샘플 폴더 재구조화: `Samples~/SettingsAndLocalization/` 래퍼 제거 → `Samples~/{Localization,Settings,UI}` 최상위. `package.json` samples `path`를 `Samples~`로 조정해 통합 1개 데모로 Import(교차 참조되는 `Frame.prefab` 포함 안전).

## [1.3.0] - 2026-07-07

### Added
- **UI** (`Border.UI`): 재사용 UI 보조 컴포넌트 2종 — `UISelectionFrameHook`(EventSystem Select/Deselect로 9-slice 선택 프레임 토글), `SelectableExtraGraphics`(Selectable 색상 트랜지션을 보조 Graphic에 동기화). Drilling에서 편입, 게임 결합 없음.
- **Samples**: `Settings & Localization Demo`에 UI/Settings/Localization 프리팹 편입 — `GenericButton`, `LocalizeText`, `LocalizeManager`, `Frame`, `Settings Panel`, `SettingsBool`, `SettingDropdown`, `SettingSlider`, `SettingsSystem`. 이벤트 채널/`SettingsSO`/`LocalizationTable` 참조는 asset GUID 보존으로 샘플 SO에 그대로 연결됨.

### Changed
- 샘플 프리팹에서 게임 종속 요소 제거: 사운드 훅(`UISelectableSoundHook`) 컴포넌트 스트립, 게임 폰트(Mulmaru)→`LiberationSans SDF`, 게임 스프라이트(13종)→builtin `UISprite`, 폐기 참조(HUD 채널/게임 SaveLoadSystem)→null. 컴파일·컴포넌트 결합 0.

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
