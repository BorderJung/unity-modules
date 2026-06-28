# Changelog

All notable changes to `com.borderjung.core` are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/), versioning follows [SemVer](https://semver.org/).

## [1.0.0] - 2026-06-15
### Added
- `Log` — `[Conditional("UNITY_EDITOR")]` logging facade (`D`/`W`/`E` with optional context).
- `DeterministicRng` — GC-free xorshift32 PRNG (`Reseed`, `Next`, `NextDouble`).
- `ScreenshotManager` — hotkey screenshot capture to `persistentDataPath`.
