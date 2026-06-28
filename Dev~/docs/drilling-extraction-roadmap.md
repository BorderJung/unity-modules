# Drilling → unity-modules: Packaging Extraction Roadmap

> Source project: `C:/Users/jungs/LocalRepo/00_Drilling/Drilling/Assets` (the game "Drilling").
> Method: a 97-agent audit — classify every candidate area → **adversarially refute** each "packageable" claim → independent **completeness critique**. Scores below are the *verified* (post-refutation) scores, not the optimistic first pass.
> This roadmap is the input to §8 of [CLAUDE.md](../CLAUDE.md) (one-module-at-a-time extraction). Nothing here is extracted yet.

## Key facts that shape everything

- **No asmdefs exist anywhere in `02_Scripts`.** The whole game compiles into `Assembly-CSharp`. So *every* module needs a `Border.*` asmdef + namespace authored from scratch, and "couplings" are pervasive only because there is no assembly boundary yet — most are easy to carve.
- **`DescriptionSO` is a game-wide SO base** (`02_Scripts/BaseClass/DescriptionSO.cs`) that ~25 types derive from (every event channel, pool, factory, anchor, AudioCueSO…). The **already-extracted library dropped `DescriptionSO`** and reparented those types to plain `ScriptableObject`. → Decision needed (see Open Decisions): do the same here, or ship a minimal base.
- **DOTween, Cinemachine, TMP, Newtonsoft, URP Light2D, Input System, Steamworks** are external couplings to *record as dependencies*, never to package. DOTween in particular touches many UI/transform utilities — a strategic friction point for a portfolio library (see Open Decisions).
- The four user-named priorities all exist and are extractable: **Settings**, **Audio**, **Event Channels**, **Excel/Sheet import** (see mapping at the end).

---

## Verified package catalog

Score = verified reusability (5 = drop-in, 3 = reusable after moderate decoupling, ≤2 = mostly game-specific). Effort = extraction+decoupling work.

| Package (proposed) | Namespace | Source (Drilling) | Score | Kind | Internal deps | External deps | Effort |
|---|---|---|---|---|---|---|---|
| **events** | `Border.Events` | `02_Scripts/Events` (primitives) + `BaseClass/DescriptionSO` | 4 | Runtime+Editor | — | — | M |
| **runtime-anchor** | `Border.RuntimeAnchor` | `02_Scripts/RuntimeAnchor` | 3 | Runtime | core (Log) | — | S |
| **pool** | `Border.Pool` | `02_Scripts/Pool/BaseClass` + `Factory` | 4 | Runtime | core | — | M |
| **transform-utils** | `Border.TransformUtils` | `02_Scripts/Transform`, `Camera/CameraParallax` | 4 | Runtime | runtime-anchor (TargetFollower) | DOTween (Floating/Track), Cinemachine (Track) | M |
| **camera** | `Border.Camera` | `02_Scripts/Camera` | 4 | Runtime | — | Cinemachine (noise) | M |
| **ui-widgets** | `Border.UI` | `02_Scripts/UI/*`, `UI/Effect`, `Shop/HelpBlink` | 4 | Runtime | — | DOTween, TMP | M |
| **ui-fade** | `Border.UI.Fade` | `UI/FadeController` + `Events/FadeChannelSO` | 4 | Runtime | events | DOTween | S |
| **input** (extend) | `Border.Input` | `02_Scripts/UI/FocusManager`, `Manager/DualSenseHapticManager` | 4–5 | Runtime | — | Input System | M |
| **editor-tools** (extend) | `Border.EditorTools` | `Editor/Tools/ReferenceExplorer*`, `OpenDataPath`, `ScriptTemplates` | 4 | Editor | — | — | S |
| **core** (extend) | `Border.Core` | `Chunk/DeterministicRng`, `Manager/Util/ScreenShotManager` | 5 | Runtime | — | — | S |
| **localization** | `Border.Localization` | `02_Scripts/Localization` + `Editor/Localization` | 3–4 | Runtime+Editor | events (String channel) | TMP (UILocalizeText) | M |
| **audio** | `Border.Audio` | `02_Scripts/Audios` (+ `03_Prefabs/Audio`) | 3–4 | Runtime | events, pool | UnityEngine.Audio | L |
| **settings** | `Border.Settings` | `02_Scripts/Settings` (+ prefab/SO) | 3 | Runtime | events, **save abstraction** | — | L |
| **sheet-importer** | `Border.SheetImporter` | `02_Scripts/Data/GoogleSheet` | 3 | Runtime+Editor | — | Newtonsoft.Json | L |
| **rope2d** *(optional)* | `Border.Rope2D` | `Effects/CableLine2D` | 4 | Runtime | — | — | S |
| **save-load** (upgrade) | `Border.SaveLoad` | `02_Scripts/SaveLoadSystem` (FileManager + migration) | 4 | Runtime | core | — | M |

S ≈ <½ day · M ≈ ~1 day · L ≈ multi-day (real decoupling).

---

## Tier 0 — SO-architecture foundation (extract first; many packages depend on these)

### `com.borderjung.events` — SO event channels  ★ user priority #3
- **Ship**: `VoidEventChannelSO`, `Bool/Int/Float/Vector2/String` channels, plus `FadeChannelSO` if not split into ui-fade. Generic Unity-Open-Project-style channels (`RaiseEvent()` + typed `UnityAction`).
- **Leave behind** (Drilling-specific, same folder): `PlayerStatType*`, `Compass*`, `TileBroken*`, `GameState*`, `AudioCue*` channels.
- **Decouple**: the channels currently derive from `DescriptionSO`. Per the library's existing convention, reparent to plain `ScriptableObject` (drop the `[TextArea] Description`). Add `Border.Events` namespace + asmdef.
- This is the **keystone** — localization, ui-fade, settings, audio all depend on it. Extract it right after `core`.

### `com.borderjung.runtime-anchor` — runtime reference SO
- **Ship**: `RuntimeAnchorBase<T>` (Provide/UnSet, `IsSet`, `OnAnchorProvided`), `TransformAnchor`.
- **Decouple**: only dep is `Log` (→ core) and `[ReadOnly]`. Drop `DescriptionSO` base.

### `com.borderjung.pool` — SO object pool
- **Ship**: `IPool<T>`, `IFactory<T>`, `PoolSO<T>`, `ComponentPoolSO<T>`, `GameObjectPoolSO`, `FactorySO<T>`, `GameObjectFactorySO`, `ComponentFactory`, `PoolRegistry`, `IClearablePool` (domain-reload-safe `ResetStatics`).
- **Leave behind**: concrete `ChunkPoolSO`, `OreIndicatorPoolSO`, `HudPoolSO`, `SoundEmitterPoolSO` (domain subclasses that *consume* this package).

> Granularity note: events + runtime-anchor + pool could be one `com.borderjung.so-architecture` umbrella. Default recommendation: keep them **separate** (cleaner catalog, each is its own portfolio talking point), with `core` at the base.

---

## Tier 1 — Clean utilities (low risk, high reuse — good early wins)

### `com.borderjung.core` (extend the existing package)
- **`DeterministicRng`** (currently buried in `Chunk/DeterministicRng.cs`): pure xorshift32 PRNG, zero Unity refs — a textbook util in the wrong folder. → core/utils.
- **`ScreenshotManager`** (`Manager/Util/ScreenShotManager.cs`): F12 → timestamped PNG to `persistentDataPath`. Tiny debug util.
- **⚠ Reconcile `Log`**: this project's `Log` gates **all** levels (incl. warnings/errors) behind `[Conditional("UNITY_EDITOR")]`, so everything is stripped from player builds. Confirm whether the already-extracted `core` Log behaves the same — they may diverge.

### `com.borderjung.transform-utils`
- `Rotator` (score 5, zero deps — could even live in core), `CameraParallax` (5), `FloatingEffect` (4, DOTween), `TargetFollower` (→ runtime-anchor), `TrackMover` (waypoint dolly + scene-view gizmos; DOTween + optional Cinemachine reveal; has an editor handle script).

### `com.borderjung.camera`
- `CameraParallax`, `CameraNoiseLayer`/`CameraNoiseBuilder` (Cinemachine-3 noise synthesis), `CameraZoomOut`, `CameraZoomTrigger`, `PlayerBoundaryBuilder2D` (5). External: Cinemachine for the noise helper only.

### `com.borderjung.editor-tools` (extend)
- `ReferenceExplorer` (GUID-based find-references window, score 4), `OpenDataPath` (reveal `persistentDataPath`), generalized `ScriptTemplates` token-replacement (`#SCRIPTNAME#`). Port only the reusable `CreateMenuButton` helper from `MainToolbarButtons` (rest is Drilling-branded).

### `com.borderjung.ui-widgets`
- `SelectableExtraGraphics` (5, syncs extra Graphics to a uGUI Selectable's color states), `UIMenuButton` (4), `TMPAlphaBlinkEffect` (4), `UISelectionFrameHook`, `HelpBlink`. External: DOTween, TMP.

### `com.borderjung.ui-fade`
- `FadeController` + `FadeChannelSO` (fullscreen Image fade in/out via event channel). Depends on **events**. External: DOTween.

### `com.borderjung.input` (extend the existing package)
- **`FocusManager`** (score 4): mouse-vs-gamepad activity polling, device-family classification (KB&M / Gamepad / PlayStation / SwitchPro), `EventSystem` selection sync. External: Input System.
- **`DualSenseHapticManager`** (score 5, clean): DualSense haptics — niche but drop-in. Optional sub-feature or its own tiny package.

---

## Tier 2 — Semi-generic systems (real decoupling required; the user's priorities)

### `com.borderjung.settings` — settings menu  ★ user priority #1
- **Ship**: `UISettingsSlider` (5), `UISettingsDropdown` (4), `UISettingsCheck`, `UISettingsAudioComponent`, `UISettingsGraphicsComponent`, `SettingsGraphicsUtility` (resolution/quality apply).
- **Decouple (the hard part)**: the system is bound to this project's `SaveLoadSystem` + `SettingsSO`. Invert via an `ISettingsStore` interface (or an event channel) so persistence is the consumer's choice. Depends on **events**. The individual widgets are far cleaner than the audio/graphics aggregate components — consider shipping widgets first.

### `com.borderjung.audio` — audio system  ★ user priority #2
- **Ship**: `SoundEmitter`, `AudioConfigurationSO` (must travel *with* SoundEmitter), `AudioCueSO`, `SoundEmitterVault`, `AudioManager`, `AudioCueEventChannelSO`, plus the audio pool. Unity-Open-Project-style architecture.
- **Decouple**: (1) bundle `AudioConfigurationSO` into the package — `SoundEmitter` won't compile without it; (2) **strip the `DefaultAudioMixer.mixer` reference** from the shipped prefab (guid won't travel → broken import) or document that consumers assign their own `AudioMixerGroup`; (3) drop `DescriptionSO` base from `AudioCueSO`; (4) the "cave acoustics" low-pass/reverb blend in `SoundEmitter` is mechanically a generic *enclosed-space DSP* toggle — rename/generalize, don't treat as domain. Depends on **events** + **pool**.

### `com.borderjung.localization`
- **Ship**: `LocalizationManager`, `LocalizationTable`/`Entry`/`LocalizedTextPair`, `[LocalizeKey]` attribute + `LocalizeKeyDrawer` (searchable key picker), `UILocalizeText`, `UILocalizeTextEditor`.
- **Decouple**: only runtime coupling is `StringEventChannelSO` → depends on **events**. `UILocalizeText` needs TMP. Needs an editor asmdef for the drawers.

### `com.borderjung.sheet-importer` — Google Sheet/Excel import  ★ user priority #4
- **Ship**: the shared importer skeleton in `02_Scripts/Data/GoogleSheet` — `SecretData` URL fetch (HttpClient), case-insensitive sheet/column alias matching, `TryReadString/Int`, `EnsureOutputFolder`, asset upsert-by-id.
- **Decouple to match the user's vision** ("API key/folder configurable + subclass to extend"): extract a `SheetImporterBase` (config = sheet id + output folder + secret) with virtual parse/map hooks; the per-sheet managers (`AlterSheetManager`, `ItemSheetManager`, …) become **subclasses** that map columns → their own SO types and stay in the game. External: Newtonsoft.Json. Do **not** package `Secret.json` or the per-manager custom editors.

---

## Tier 3 — Optional / niche

- **`com.borderjung.rope2d`** — `CableLine2D` Verlet rope→`EdgeCollider2D` (score 4). Decouple: hardcoded `"Player"` tag → serialized field.
- Effects: `CableElectricArcVFX`, `FireflyFadeEffect` (URP Light2D), `CaveEdgeFadeSettings` — niche, evaluate later.

---

## Existing-package overlaps

- **save-load**: this project's version is **more advanced** than the extracted one — it has `SaveVersion` / `SaveVersionCatalog` / `SaveMigrationPipeline` (versioned save migration) on top of `FileManager`. Recommend **upgrading** the extracted `save-load` package with these.
- **scene-management / input**: project versions exist; `SplashLoader` (timeline-gated transition) is not worth packaging. FocusManager/DualSense extend input (above).
- **core/Log**: behavioral divergence flagged above — reconcile.

## NOT worth packaging (game-specific or low value)
`SecretData` alone · per-manager sheet editors · JSON→SO code generator · `SplashLoader` · Dialog/Inventory/Cost/Reward systems · Floating HUD · most of `Manager/` · `GradientPalette` · `BedrockUpscale` · Drilling-branded `MainToolbarButtons`.

---

## Recommended extraction order (dependency-sorted, one at a time per CLAUDE.md §8)

1. **core** (add DeterministicRng + ScreenshotManager; reconcile Log) — foundation, no deps
2. **events** — keystone; many depend on it
3. **runtime-anchor**, **pool** — depend only on core
4. **transform-utils**, **camera**, **ui-widgets**, **editor-tools**+, **input**+ — leaf utilities
5. **ui-fade**, **localization** — depend on events
6. **settings**, **audio** — depend on events (+ pool / save abstraction); biggest decoupling
7. **sheet-importer** — Newtonsoft
8. **save-load** upgrade (supersede with FileManager + migration)

**Suggested first concrete target after `core`: `events`** — it's the user's priority #3, it's the keystone the most other packages need, and it's a clean Tier-0 extraction that exercises the full §8 pipeline (asmdef + namespace + demo scene + tag) on something low-risk.

---

## Decisions (confirmed 2026-06-15)

1. **`DescriptionSO`** → **reparent channels/SOs to plain `ScriptableObject`** (drop the description field), matching the already-extracted library. No shared base type across packages.
2. **DOTween** → **core stays dependency-free**; keep DOTween only in pure *presentation* utilities (fade/blink/floating/track), declared per-package. Tier-0 infra (events/pool/runtime-anchor/settings core) must have zero DOTween.
3. **Granularity** → **separate small packages** (events / runtime-anchor / pool each standalone), `core` at the base.
4. **Start** → **`core` → `events`** first, then proceed down the recommended order.

## User-priority mapping
| Your ask | Package | Tier / effort |
|---|---|---|
| 1. 설정창 (audio/graphics settings) | `com.borderjung.settings` | Tier 2 / L |
| 2. 오디오 시스템 | `com.borderjung.audio` | Tier 2 / L |
| 3. 이벤트 채널 (옵저버) | `com.borderjung.events` | Tier 0 / M |
| 4. 엑셀/시트 임포트 (확장형) | `com.borderjung.sheet-importer` | Tier 2 / L |
