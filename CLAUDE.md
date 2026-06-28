# unity-modules — Working Guidelines

> A library of reusable Unity modules shipped as **one embedded UPM package** — `com.borderjung.unity-modules`.
> Two goals — (1) battle-tested modules you drop into any project with a single git URL, (2) a portfolio piece demonstrating "design & operation of a reusable module library."
> This document is the **single source of truth** for all work (human or agent). Read it before touching code.

---

## 0. Core Decisions (update this table first if any change)

| Item | Value |
|---|---|
| GitHub repo | `https://github.com/BorderJung/unity-modules` |
| Distribution | **single UPM package** `com.borderjung.unity-modules` — **repo root = package root** |
| Install (consumer) | bare git URL, **no `?path=`**: `https://github.com/BorderJung/unity-modules.git#vX.Y.Z` |
| Code namespace / brand | `Border.*` (PascalCase) |
| Default assembly | `Border` — single asmdef at `Runtime/Border.asmdef` |
| Minimum Unity version | `2021.3` (LTS) |
| Versioning | SemVer + git tag (`vX.Y.Z`), **one tag for the whole package** |

> **History (why single-package):** this started as a multi-package monorepo (`Packages/com.borderjung.*` consumed via `?path=`). It was **consolidated into ONE root package** so consumers import *everything* with a bare git URL — no `?path`, no per-module dependency wiring in their manifest. The old per-module scaffolding is archived under `Dev~/`. **Do not re-split into multiple packages** (§10).

---

## 1. Repository Layout

```
unity-modules/                 = the UPM package (repo root)
├─ package.json                # com.borderjung.unity-modules
├─ README.md / CHANGELOG.md / LICENSE.md   (+ .meta each)
├─ CLAUDE.md                   # this file (+ .meta)
├─ .gitignore
├─ Runtime/
│   ├─ Border.asmdef           # single assembly, references: []
│   ├─ Core/    → namespace Border.Core    (Log, DeterministicRng, ScreenshotManager)
│   └─ Events/  → namespace Border.Events  (ScriptableObject event channels)
├─ Editor/                     # only if a module needs editor code (Border.*.Editor asmdef)
├─ Samples~/                   # optional sample assets (~ = not auto-imported, Package Manager button)
└─ Dev~/                       # ★ Unity IGNORES ~ folders entirely. Git-tracked staging area:
    ├─ Packages/               #   old per-module package scaffolding (archived)
    ├─ Assets/ ProjectSettings/#   old dev-project scaffolding
    ├─ docs/                   #   drilling-extraction-roadmap.md
    └─ FSM/ Input/ SaveLoadSystem/ SceneManagement/ EditorTools/   # not-yet-folded modules
```

**Key:** anything under a `~`-suffixed folder (`Dev~/`, `Samples~/`) is committed to git but invisible to Unity. `Dev~/` is the holding pen for modules and assets not yet folded into the package.

---

## 2. The Single-Assembly Model

- All runtime code compiles into **one assembly `Border`** (`Runtime/Border.asmdef`, `autoReferenced: true`, `references: []`). Consumers just `using Border.Core;` etc. — no manual assembly references.
- Each **module = a namespace + a folder** under `Runtime/`: `Border.Core` in `Runtime/Core/`, `Border.Events` in `Runtime/Events/`, and so on.
- A module gets its **own asmdef** (`Border.<Module>`) **only when** it needs:
  - an **external Unity package** (InputSystem, Addressables, …), or
  - **editor-only code** → `Border.<Module>.Editor` with `includePlatforms: ["Editor"]`.
  Otherwise keep it in the single `Border` assembly. Don't create asmdefs you don't need.

---

## 3. Naming Conventions

| Target | Rule | Example |
|---|---|---|
| Package id | fixed — `com.borderjung.unity-modules` | — |
| Module folder | `Runtime/<Module>/` (PascalCase) | `Runtime/SaveLoad/` |
| Namespace | `Border.<Module>` | `Border.SaveLoad` |
| Default asmdef | `Border` (everything dependency-free lives here) | `Border` |
| Per-module asmdef (only if needed) | `Border.<Module>` | `Border.Input` |
| Editor asmdef | `Border.<Module>.Editor` (`includePlatforms:["Editor"]`) | `Border.SaveLoad.Editor` |
| Tests asmdef | `Border.<Module>.Tests` (+`.Editor`) | `Border.SaveLoad.Tests` |

---

## 4. Package Composition

### `package.json` (single, at repo root)
```json
{
  "name": "com.borderjung.unity-modules",
  "version": "1.0.0",
  "displayName": "Border Unity Modules",
  "description": "Reusable Unity modules by BorderJung, bundled as one package.",
  "unity": "2021.3",
  "dependencies": {},
  "author": { "name": "BorderJung", "url": "https://github.com/BorderJung" }
}
```
- External Unity deps (InputSystem, Addressables, …) go in `dependencies` — **but read the §5 tradeoff first** (a package-level dep is forced on *every* consumer).

### Default runtime asmdef — `Runtime/Border.asmdef`
```json
{ "name": "Border", "rootNamespace": "Border", "references": [], "autoReferenced": true }
```

### Per-module asmdef (only if it needs deps/editor split) — `Runtime/<Module>/Border.<Module>.asmdef`
```json
{ "name": "Border.Input", "rootNamespace": "Border.Input", "references": ["Border"], "autoReferenced": true }
```
> The asmdef `references` field is what **enforces dependencies at compile time**. If a module references project code, compilation breaks — so "a module doesn't know the project that consumes it" (one-way dependency) is upheld.

### Editor asmdef — `Editor/<Module>/Border.<Module>.Editor.asmdef`
```json
{ "name": "Border.SaveLoad.Editor", "references": ["Border"], "includePlatforms": ["Editor"] }
```

---

## 5. Dependency Rules (the single-package tradeoff)

1. **Package-level deps hit everyone.** `package.json` `dependencies` are installed for *every* consumer of this package. So a module that needs Addressables would force Addressables onto a project that only wanted `Core`. Keep this in mind before adding any external dep.
2. **Keep the core lean.** Dependency-free modules (Core, Events, …) live in the `Border` assembly with **no** deps. For a module that needs an external Unity package, prefer one of:
   - **Optional (preferred):** give the module its own asmdef and gate it with `defineConstraints`/`versionDefines` so it **compiles only when the dep is present**, and do **not** add the dep to `package.json`. Consumers who want that module add the dep themselves.
   - **Forced (simple):** add the dep to `package.json` so every consumer gets it. Use only for deps you're comfortable forcing on everyone.
3. **One-way only.** Module → module deps flow top-down, no cycles. Shared code converges into `Border.Core`.
4. **Editor code stays editor-only.** `Editor/` + `includePlatforms:["Editor"]`. Never referenceable from runtime. (Don't repeat the old "Log living in EditorTools" mistake — runtime utilities belong in `Border.Core`.)

---

## 6. Consuming from a Project (`manifest.json`)

```json
{
  "dependencies": {
    "com.borderjung.unity-modules": "https://github.com/BorderJung/unity-modules.git#v1.0.0"
  }
}
```
- **Bare URL, no `?path=`** — the repo root *is* the package.
- `#vX.Y.Z` pins by git tag. **Without a tag you pull the latest `main` and builds drift.**
- **One entry** — the package bundles every module, so there are no internal deps to list.
- **Local development:** point at the working copy with a `file:` path (mutable — see §8B). Remember to switch back to the git URL + tag before relying on a build.

---

## 7. Versioning / Releases

- **SemVer**: breaking → MAJOR / new module or feature → MINOR / bugfix → PATCH.
- Release: bump `version` in `package.json` → update `CHANGELOG.md` → `git tag vX.Y.Z && git push --tags`.
- **One package, one tag.** All modules ship together.

---

## 8. Folding a New Module Into the Package

Two entry paths. **8A** is the usual case (you already have working code + assets + metas in another Unity project). **8B** covers brand-new code authored directly here.

### 8A. Importing from another Unity project — *bring the `.meta` files!*

> The golden rule: **copy every asset together with its sibling `.meta`, and never regenerate a meta you already have.** A `.meta` carries the asset's **GUID**; bringing it preserves every cross-reference. Regenerating = new GUID = broken references.

1. **Copy `.cs`/asset + its `.meta` together.** Why it matters: a ScriptableObject `.asset` points at its script via the *script's* GUID; a prefab points at scripts/sub-assets by GUID; an asmdef references another asmdef by `GUID:…`. Bring the metas and all of these stay intact across the move.
2. **Place + namespace.** Move into `Runtime/<Module>/`; rename the namespace to `Border.<Module>` (a code edit — does **not** touch GUIDs/metas). Strip any source-project namespace (e.g. the old FSM `Maggi.*`, or global/no-namespace).
3. **Cut the coupling.** Delete references to the source project: asmdef `references` that point at the game's assemblies, `using` of game-specific types. Re-point to `Border` (for `Log` etc.) or to declared external Unity assemblies. Invert remaining couplings via interfaces/events. Replace `Debug.Log` with `Border.Core` `Log.D/W/E`.
4. **asmdef decision** (see §2):
   - Source module had its own asmdef **and** needs separate compilation (external dep / editor split) → **keep** it (asmdef + its `.meta`), rename to `Border.<Module>`, fix `references`.
   - Otherwise → **delete** the source asmdef + its `.meta` so the code folds into the single `Border` assembly. First make sure no other asmdef referenced its GUID.
5. **Asset placement.** Editor UI assets (`.uxml`/`.uss`) → `Editor/<Module>/` under the editor asmdef. Sample/preset `.asset`/prefabs → `Samples~/<Module>/` (not auto-imported). Everything **with its `.meta`**.
6. **Declare deps / version / docs.** External Unity deps per §5. Bump `package.json` version (MINOR), update `CHANGELOG.md` and the README module list.
7. **Sanity: GUID uniqueness.** Astronomically unlikely to collide, but a quick scan for duplicate `guid:` across the package's metas is cheap insurance.
8. **Commit (with the metas), push, tag** `vX.Y.0`.

### 8B. Authoring new code directly here (no metas yet)

Root-package code is **not** auto-meta'd by opening this repo in Unity — Unity only manages `Assets/` and `Packages/`, not arbitrary root folders. To get metas:
- **Recommended — `file:` workflow.** In a Unity project (e.g. a scratch project or EmptyHouse), reference this package by a local path:
  ```json
  "com.borderjung.unity-modules": "file:C:/Users/jungs/00_LocalRepo/04_unity-modules"
  ```
  A `file:` package is **mutable**, so Unity generates/maintains `.meta` files in the repo folder and you can edit code live. Commit the generated metas, then switch consumers back to the git URL + tag.
- **Fallback — hand-generate.** Write each `.meta` with a unique 32-hex GUID and **no BOM** (folders → `folderAsset: yes` + `DefaultImporter`; `.cs` → `MonoImporter`; `.asmdef` → `AssemblyDefinitionImporter`; `package.json` → `PackageManifestImporter`; text → `TextScriptImporter`).

### Per-module checklist
- [ ] code under `Runtime/<Module>/`, namespace `Border.<Module>`
- [ ] every file has its `.meta` (brought over, or generated)
- [ ] coupling to the source project cut (asmdef refs, usings, `Debug.Log`→`Log`)
- [ ] asmdef decision made (folded into `Border`, or own asmdef for deps/editor)
- [ ] external deps handled per §5 (optional vs forced)
- [ ] `package.json` version bump + `CHANGELOG.md` + README updated
- [ ] commit + push + `git tag vX.Y.Z` + import regression check in a consumer

---

## 9. Code Conventions

- **Namespace required**: all runtime/editor code under `Border.<Module>`. No global namespace.
- **Logging**: use `Border.Core`'s `Log.D/W/E` instead of `UnityEngine.Debug.Log`. Being `[Conditional("UNITY_EDITOR")]`, it is stripped from builds automatically.
- **Prefer ScriptableObject-based design** (like Events) — split data/config into assets.
- **Editor code under `Editor/`** + an Editor-only asmdef. Never mix with runtime code.
- **Always commit `.meta` files.** A package without metas has its scripts/asmdefs silently ignored on import.

---

## 10. Do / Don't

**Do**
- Fold **one module at a time** → verify it compiles in a consumer → tag. Keep the blast radius small.
- **Bring `.meta` files** when importing from another project (preserves GUIDs).
- Converge shared code into `Border.Core`; keep the core dependency-free.
- In the README, note for each module "which project it came from, **why**, and what problem it solved" → interview talking points.

**Don't**
- **Re-split into multiple packages.** We consolidated on purpose; bare-URL import is the whole point.
- **Regenerate metas you already have** — it changes GUIDs and breaks references.
- Force heavy external deps on all consumers without weighing §5.
- Put runtime code in an Editor assembly. Consume `main` without a tag.

---

## 11. Status & Next Actions

**Source project for extraction**: the game "Drilling" at `C:/Users/jungs/LocalRepo/00_Drilling/Drilling/Assets` (Unity 6000.3.12f1). A 97-agent audit produced the packaging plan in [Dev~/docs/drilling-extraction-roadmap.md](Dev~/docs/drilling-extraction-roadmap.md) — dependency-ordered, with per-module decoupling notes. **Read it before folding anything new.** (Note: the roadmap was written for the *old* multi-package plan — translate its "package" units into *modules* under `Runtime/<Module>/` in the single-package model.)

**Confirmed decisions (2026-06-15)**
- `DescriptionSO` → all SOs reparented to plain `ScriptableObject` (no shared base type).
- DOTween → `Core` and Tier-0 infra stay dependency-free; DOTween allowed only in presentation utilities (fade/blink/floating/track), gated per §5.
- Granularity → small, self-contained modules (events / runtime-anchor / pool independent), all inside the one package.

**Done**
- **Consolidated into a single root package `com.borderjung.unity-modules` v1.0.0** — repo root is the package, `Runtime/Core` + `Runtime/Events` under the single `Border` assembly, all `.meta` committed, non-package material moved to `Dev~/`.
- Imports via **bare git URL** (`…unity-modules.git`), verified working in the EmptyHouse project.
- `Border.Core` — `Log`, `DeterministicRng`, `ScreenshotManager`. `Border.Events` — Void/Bool/Int/Float/Vector2/String channels + Fade/FloatingHud.

**Next actions**
1. `git tag v1.0.0 && git push --tags` to pin the current working state, then switch consumers to `#v1.0.0`.
2. Fold the next module from `Dev~/` (or directly from the Drilling project) per **§8A** — bring the metas, place under `Runtime/<Module>/`, decide asmdef, bump to v1.1.0.
3. Still-loose staging in `Dev~/` (FSM/Input/SaveLoadSystem/SceneManagement/EditorTools) awaits folding (the project's save-load supersedes the old one; FocusManager/DualSense extend input).
