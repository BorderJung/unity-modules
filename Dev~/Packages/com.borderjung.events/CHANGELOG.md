# Changelog

All notable changes to `com.borderjung.events` are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/), versioning follows [SemVer](https://semver.org/).

## [1.0.0] - 2026-06-15
### Added
- Primitive event channels: `VoidEventChannelSO`, `BoolEventChannelSO`, `IntEventChannelSO`,
  `FloatEventChannelSO`, `Vector2EventChannelSO`, `StringEventChannelSO`.
- Generic compound channels: `FadeChannelSO` (`bool`/`float`/`Color` with `FadeIn`/`FadeOut`),
  `FloatingHudEventChannelSO` (`string`/`Color`/`Vector3` floating world-space text).
- All channels derive from plain `ScriptableObject` (no shared base) and carry `Border/Events/*`
  create-asset menu entries.
