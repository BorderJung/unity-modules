# unity-modules

A library of reusable Unity modules, managed as a **monorepo of embedded UPM packages**. Each
module is a self-contained package under [`Packages/`](Packages/), developed and verified inside
this repo (which is itself a Unity project) and consumed from other projects via Git URL.

- **Package id**: `com.borderjung.<module>` · **Namespace/asmdef**: `Border.<Module>` · **Min Unity**: 2021.3
- Conventions & extraction process: [CLAUDE.md](CLAUDE.md)
- Where these modules come from & what's next: [docs/drilling-extraction-roadmap.md](docs/drilling-extraction-roadmap.md)

## Module catalog

| Module | Package | Status | Summary |
|---|---|---|---|
| Core | `com.borderjung.core` | ✅ v1.0.0 | Build-stripped `Log`, deterministic xorshift32 RNG, dev helpers. Zero deps. |
| Events | `com.borderjung.events` | ✅ v1.0.0 | ScriptableObject event channels (Void/Bool/Int/Float/Vector2/String). Observer pattern, zero deps. |
| Runtime Anchor | `com.borderjung.runtime-anchor` | 🔜 planned | `RuntimeAnchorBase<T>` runtime-reference SO pattern. |
| Pool | `com.borderjung.pool` | 🔜 planned | SO-driven object pool + factory + registry. |
| Save / Load | `com.borderjung.save-load` | 🔜 planned | JSON save/load with versioned migration. |
| FSM | `com.borderjung.fsm` | 🔜 planned | ScriptableObject state machine + transition-table editor. |
| Localization | `com.borderjung.localization` | 🔜 planned | Key/fallback localization + `[LocalizeKey]` picker. |
| Audio | `com.borderjung.audio` | 🔜 planned | SoundEmitter / AudioCue / pooled audio. |
| Settings | `com.borderjung.settings` | 🔜 planned | Event-driven audio/graphics settings UI. |
| Sheet Importer | `com.borderjung.sheet-importer` | 🔜 planned | Google-Sheet → ScriptableObject importer, subclass to extend. |
| Editor Tools | `com.borderjung.editor-tools` | 🔜 planned | Find-references, persistent-data-path, missing-script finder. |
| …more | | 🔜 | transform-utils, camera, ui-widgets, ui-fade, input, rope2d — see roadmap. |

## Consuming a module from another project

Add to that project's `Packages/manifest.json` (pin a tag — never consume `main` untagged):

```json
{
  "dependencies": {
    "com.borderjung.events": "https://github.com/BorderJung/unity-modules.git?path=Packages/com.borderjung.events#v1.0.0"
  }
}
```

List internal dependencies explicitly too — UPM does not auto-resolve a monorepo's internal
package deps (e.g. a package that depends on `com.borderjung.core` requires `core` to be listed
as well).

## Developing here

Open this folder in Unity (6000.3.12f1). Embedded packages under `Packages/` load automatically;
demo scenes for verifying each module live in [`Assets/Demo/`](Assets/Demo/).
