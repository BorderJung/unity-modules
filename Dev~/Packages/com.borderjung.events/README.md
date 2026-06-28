# Border Events (`com.borderjung.events`)

ScriptableObject **event channels** for decoupled, observer-style design. A channel is an asset:
one system raises it, any number of systems listen, and they are wired in the inspector — so the
publisher and subscriber never hold a hard reference to each other.

## Channels

| Asset (Create ▸ Border ▸ Events) | Type | Payload |
|---|---|---|
| Void | `VoidEventChannelSO` | — |
| Bool | `BoolEventChannelSO` | `bool` |
| Int | `IntEventChannelSO` | `int` |
| Float | `FloatEventChannelSO` | `float` |
| Vector2 | `Vector2EventChannelSO` | `Vector2` |
| String | `StringEventChannelSO` | `string` |
| UI ▸ Fade Channel | `FadeChannelSO` | `(bool fadeIn, float duration, Color)` — `FadeIn`/`FadeOut` helpers |
| Floating Hud | `FloatingHudEventChannelSO` | `(string text, Color, Vector3 worldPos)` — floating world-space text |

## Usage

```csharp
using Border.Events;

// Publisher — holds a [SerializeField] reference to the channel asset.
[SerializeField] private VoidEventChannelSO onPlayerDied;
void Die() => onPlayerDied.RaiseEvent();

// Subscriber — references the same asset, subscribes in OnEnable / unsubscribes in OnDisable.
[SerializeField] private VoidEventChannelSO onPlayerDied;
void OnEnable()  => onPlayerDied.OnEventRaised += HandleDeath;
void OnDisable() => onPlayerDied.OnEventRaised -= HandleDeath;
```

## Notes

- Channels derive from plain `ScriptableObject` (no shared base type), so this package has
  **zero dependencies** — drop it into any project.
- `OnEventRaised` is initialized to an empty delegate, so raising a channel with no listeners is safe.
- Always unsubscribe in `OnDisable`/`OnDestroy`; channel assets outlive scene objects.

## Install

```
"com.borderjung.events": "https://github.com/BorderJung/unity-modules.git?path=Packages/com.borderjung.events#v1.0.0"
```
