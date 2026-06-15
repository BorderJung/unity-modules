# unity-modules — Working Guidelines

> A library of reusable Unity modules managed as a **monorepo of embedded UPM packages**.
> Two goals — (1) battle-tested modules to drop into any project, (2) a portfolio piece demonstrating "design & operation of a reusable module library."
> This document is the **single source of truth** for all work (human or agent). Read it before touching code.

---

## 0. Core Decisions (update this table first if any change)

| Item | Value |
|---|---|
| GitHub repo | `https://github.com/BorderJung/unity-modules` |
| UPM package id prefix | `com.borderjung.*` (lowercase, from gh-id) |
| Code namespace / asmdef brand | `Border.*` |
| Minimum Unity version | `2021.3` (LTS) |
| Versioning | SemVer + git tag (`v1.0.0`) |

> Package id (`com.borderjung.core`) and code brand (`Border.Core`) are **two different concepts**. The former is UPM reverse-DNS (lowercase); the latter is the asmdef name / rootNamespace (PascalCase). Both are fixed by the rules above.

---

## 1. Target Repository Layout

```
unity-modules/                       # is itself a single Unity project
├─ Packages/
│   ├─ com.borderjung.core/          # the one home for shared code
│   │   ├─ package.json
│   │   ├─ README.md / CHANGELOG.md / LICENSE.md
│   │   ├─ Runtime/  Border.Core.asmdef + *.cs
│   │   ├─ Editor/   Border.Core.Editor.asmdef (if needed)
│   │   ├─ Tests/    Runtime/ Editor/  (each *.Tests.asmdef)
│   │   └─ Samples~/                 # trailing ~ → not auto-imported (button only)
│   ├─ com.borderjung.fsm/
│   ├─ com.borderjung.input/
│   ├─ com.borderjung.save-load/
│   ├─ com.borderjung.scene-management/
│   └─ com.borderjung.editor-tools/
├─ Assets/
│   └─ Demo/                         # scenes that import each module and prove it runs
├─ ProjectSettings/
├─ .gitignore                        # Unity template (Library/ Temp/ obj/ …)
└─ README.md                         # ★ module catalog = the face of the portfolio
```

Opening the repo immediately verifies every module actually runs in a demo scene, and package development happens right inside it.

---

## 2. Module → Package Mapping (current → target)

The repo is not yet a Unity project — only **raw folders + .cs** files. Below is the extraction target.

| Current location | Package id | asmdef / namespace | Kind | External/internal deps |
|---|---|---|---|---|
| `EditorTools/Log.cs` | `com.borderjung.core` | `Border.Core` | **Runtime** | none |
| `FSM/` | `com.borderjung.fsm` | `Border.StateMachine` (+`.Editor`, `.ScriptableObjects`) | Runtime+Editor | none (UI Toolkit is built-in) |
| `Input/` | `com.borderjung.input` | `Border.Input` | Runtime | `com.unity.inputsystem` |
| `SaveLoadSystem/` | `com.borderjung.save-load` | `Border.SaveLoad` | Runtime | **`com.borderjung.core`** (Log) |
| `SceneManagement/` | `com.borderjung.scene-management` | `Border.SceneManagement` | Runtime | `com.unity.addressables` |
| `EditorTools/FindMissingScriptsInScene.cs`, `TMPFontChanger.cs` | `com.borderjung.editor-tools` | `Border.EditorTools` | **Editor** | none |

### ⚠ Traps to catch during extraction
- **`Log` lives in the EditorTools folder but is a runtime utility.** `SaveLoadSystem.FileManager` calls it at runtime, gated by `[Conditional("UNITY_EDITOR")]`. → It must NOT live in an editor assembly. Move it to `core`'s **Runtime/**, and make SaveLoad depend on core.
- **The 4 modules other than FSM have no namespace** (global). Add a `Border.*` namespace while extracting.
- **FSM's current namespace is `Maggi.StateMachine`.** Per brand unification, rename it to `Border.StateMachine` (+ `.Editor`, `.ScriptableObjects`) wholesale.

---

## 3. Naming Conventions

| Target | Rule | Example |
|---|---|---|
| Package id | `com.borderjung.<module>` (lowercase, kebab-case for multi-word) | `com.borderjung.save-load` |
| Package folder | same as package id | `Packages/com.borderjung.save-load/` |
| asmdef name = rootNamespace | `Border.<Module>` (PascalCase) | `Border.SaveLoad` |
| Editor asmdef | `Border.<Module>.Editor` | `Border.SaveLoad.Editor` |
| Tests asmdef | `Border.<Module>.Tests` (+`.Editor`) | `Border.SaveLoad.Tests` |
| `displayName` | human-readable name | `Border Save/Load` |

---

## 4. Standard Package Composition

### package.json
```json
{
  "name": "com.borderjung.save-load",
  "version": "1.0.0",
  "displayName": "Border Save/Load",
  "description": "JSON-based save/load system",
  "unity": "2021.3",
  "dependencies": {
    "com.borderjung.core": "1.0.0"
  },
  "author": { "name": "BorderJung", "url": "https://github.com/BorderJung" }
}
```
- External Unity package deps (InputSystem, Addressables, …) also go in `dependencies`.
- Inter-module deps go here too → but always read together with the monorepo constraint in **§5**.

### Runtime asmdef — `Runtime/Border.SaveLoad.asmdef`
```json
{
  "name": "Border.SaveLoad",
  "rootNamespace": "Border.SaveLoad",
  "references": ["Border.Core"],
  "autoReferenced": true
}
```
> The asmdef `references` field is what **enforces dependencies at compile time**. If a module tries to reference project code, compilation breaks — so "a module does not know the project that consumes it" (one-way dependency) is naturally upheld.

### Editor asmdef — `Editor/Border.SaveLoad.Editor.asmdef`
```json
{
  "name": "Border.SaveLoad.Editor",
  "rootNamespace": "Border.SaveLoad.Editor",
  "references": ["Border.SaveLoad"],
  "includePlatforms": ["Editor"]
}
```

---

## 5. Dependency Rules (most important)

1. **One-way**: module → module deps flow top-down only. No cycles. Shared code lives in **`core` only**.
2. **Monorepo constraint**: UPM cannot auto-resolve *internal* package deps inside a Git monorepo. So a consuming project's `manifest.json` must **also list the dependency packages directly** (see §6). → Because of this, keep inter-module deps **minimal** and converge shared code into core.
3. **External Unity packages**: declare precisely in each `package.json`'s `dependencies` (`com.unity.inputsystem`, `com.unity.addressables`, …).
4. **Separate editor code**: editor-only code goes in `Editor/` + `includePlatforms:["Editor"]`. Never referenceable from runtime. (Don't repeat the Log-in-EditorTools mistake.)

---

## 6. Consuming from a Project (`manifest.json`)

```json
{
  "dependencies": {
    "com.borderjung.core":      "https://github.com/BorderJung/unity-modules.git?path=Packages/com.borderjung.core#v1.0.0",
    "com.borderjung.save-load": "https://github.com/BorderJung/unity-modules.git?path=Packages/com.borderjung.save-load#v1.0.0"
  }
}
```
- `?path=` → import only a specific package inside the monorepo.
- `#v1.0.0` → pin a version by git tag. **Without a tag you always pull the latest main and builds become unstable.**
- save-load depends on core, so **list core too** (§5 constraint).

---

## 7. Versioning / Releases

- **SemVer**: breaking → MAJOR / feature → MINOR / bugfix → PATCH.
- Release: bump `version` in `package.json` → update CHANGELOG → `git tag v1.1.0 && git push --tags`.
- Being a monorepo, **one tag is shared by all packages**. If per-module independent releases become necessary, split then with prefixed tags like `save-load/v1.1.0` — **do not over-split from the start**.

---

## 8. Module Extraction Procedure (one at a time, least-dependent first)

> Recommended order: **core → editor-tools / input / scene-management → save-load (after core, since it depends on core) → fsm**.

For each module:
1. (If the original project is separate) attach a **temporary asmdef** to the target code and build → references that break are "dependencies leaking from the project."
2. **Invert** those deps via interfaces/events to cut them (the module depends on an abstraction, not a concrete class).
3. Move code into `Packages/com.borderjung.<module>/` + clean up the **`Border.*` namespace** + create package.json/asmdef skeletons.
4. **Verify it runs in a demo scene (`Assets/Demo/`)** → on pass, `git tag` to release.
5. Delete the code from the original project and re-import via `manifest.json` → **confirm no regression**.

**Finish verifying one module before the next.** If something breaks, the blast radius is small. Never move everything at once.

### Per-module extraction checklist
- [ ] `package.json` written (name/version/unity/dependencies/author)
- [ ] Runtime asmdef + (if needed) Editor asmdef, correct `references`
- [ ] `Border.<Module>` namespace on all .cs
- [ ] all external/internal deps declared in `dependencies`
- [ ] a working scene in `Assets/Demo/` + compile & play verified
- [ ] README.md / CHANGELOG.md (per module)
- [ ] git tag, then manifest.json import regression check

---

## 9. Code Conventions (observed in the repo)

- **Namespace required**: all runtime/editor code under `Border.<Module>`. No global namespace.
- **Logging**: use `Border.Core`'s `Log.D/W/E` instead of calling `UnityEngine.Debug.Log` directly. Being `[Conditional("UNITY_EDITOR")]`, it is stripped from builds automatically.
- **Prefer ScriptableObject-based design** (like FSM and SceneManagement) — split data/config into assets.
- **Editor code under `Editor/`** + an Editor-only asmdef. Never mix with runtime code.

---

## 10. Do / Don't

**Do**
- Extract one module at a time → verify in a demo → tag. Keep it narrow.
- Converge shared code into `core`.
- In each package/module README, write one paragraph on "which project it was extracted from, **why**, and what problem it solved" → accumulates as interview talking points.

**Don't**
- Overuse inter-module deps (especially cycles). Avoid lateral deps outside core.
- Consume main without a tag. Builds drift.
- Put runtime code in an Editor assembly (the Log case).
- Extract several modules at once.

---

## 11. Status & Next Actions

**Source project for extraction**: the game "Drilling" at `C:/Users/jungs/LocalRepo/00_Drilling/Drilling/Assets` (Unity 6000.3.12f1). A 97-agent audit produced the verified packaging plan in [docs/drilling-extraction-roadmap.md](docs/drilling-extraction-roadmap.md) — ~16 candidate packages, dependency-ordered, with per-package decoupling notes. **Read the roadmap before extracting anything new.**

**Confirmed decisions (2026-06-15)**
- `DescriptionSO` → all SOs reparented to plain `ScriptableObject` (no shared base type across packages).
- DOTween → `core` and Tier-0 infra stay dependency-free; DOTween allowed only in presentation utilities (fade/blink/floating/track), declared per-package.
- Granularity → separate small packages (events / runtime-anchor / pool standalone).
- Start order → `core` → `events` → … down the roadmap order.

**Done**
- Repo scaffolded as a Unity project: `.gitignore`, `ProjectSettings/ProjectVersion.txt` (6000.3.12f1), `Packages/manifest.json`, `Assets/Demo/`, root `README.md` catalog.
- `com.borderjung.core` v1.0.0 — `Log`, `DeterministicRng`, `ScreenshotManager`.
- `com.borderjung.events` v1.0.0 — Void/Bool/Int/Float/Vector2/String channels (plain `ScriptableObject`, `Border/Events/*` menu).

**Next actions**
1. Open the repo in Unity once to generate metas/Library and verify `core` + `events` compile; add demo scenes in `Assets/Demo/` (Core determinism, Events publisher/subscriber). Then `git tag v1.0.0`.
2. Continue down the roadmap: `runtime-anchor`, `pool` (both depend only on core).
3. The still-loose root folders (FSM/Input/SaveLoadSystem/SceneManagement/EditorTools) are pre-package staging — fold into packages per the roadmap (the project's save-load supersedes; FocusManager/DualSense extend input).
