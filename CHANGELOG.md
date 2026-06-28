# Changelog

All notable changes to `com.borderjung.unity-modules` are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/) and this project adheres to [SemVer](https://semver.org/).

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
