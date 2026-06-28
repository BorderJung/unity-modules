# Border Core (`com.borderjung.core`)

Foundational, dependency-free utilities shared across the Border module library.

## Contents

| Type | Summary |
|---|---|
| `Border.Core.Log` | Logging facade. `D`/`W`/`E` are `[Conditional("UNITY_EDITOR")]`, so **all** logs (incl. warnings/errors) are stripped from player builds at compile time. Use instead of `Debug.Log`. |
| `Border.Core.DeterministicRng` | GC-free xorshift32 PRNG. Reseedable; same seed → same sequence. For reproducible procedural generation on hot paths. |
| `Border.Core.ScreenshotManager` | Dev helper: hotkey (default F12) → timestamped PNG into `persistentDataPath`. Requires legacy input enabled. |

## Why this exists

Every other Border package leans on a tiny shared base. Convergence here keeps inter-module
dependencies minimal (see the monorepo dependency rules). `Log` in particular was extracted from
a game project where it lived — incorrectly — inside an editor-only folder despite being used at
runtime; it belongs here, in a Runtime assembly.

## Install

```
"com.borderjung.core": "https://github.com/BorderJung/unity-modules.git?path=Packages/com.borderjung.core#v1.0.0"
```

## Usage

```csharp
using Border.Core;

Log.D("hello");                     // editor only; compiled out of player builds

var rng = new DeterministicRng();
rng.Reseed(12345);
int roll = rng.Next(1, 7);          // deterministic for the given seed
```
