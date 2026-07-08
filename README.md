# Border Unity Modules

`com.borderjung.unity-modules` — BorderJung의 재사용 Unity 모듈을 **소스 딜리버리 패키지**로 묶은 것.
git URL로 추가한 뒤 **Package Manager의 Samples에서 Import** 하면, 전체 소스(스크립트 + 프리팹 + SO)가 `Assets/`로 복사되어 **편집 가능**해집니다.

- **Package id**: `com.borderjung.unity-modules`
- **Namespace**: `Border.*` / **Assembly**: `Border`, `Border.Editor`, `Border.Input`
- **Min Unity**: 2021.3
- **외부 의존성**: `com.unity.ugui` (TextMeshPro/UGUI) — Localization/Settings/UI가 사용

> **모델 (v2.0.0~): Import 전용.** 이 패키지는 `Packages/`에서 바로 컴파일되는 라이브러리가 **아닙니다.** 모든 소스는 `Plugins~/borderjung/` 안에 있어 패키지 상태에선 컴파일되지 않고, **Samples Import를 통해 `Assets/`로 복사되어야** 컴파일·사용됩니다. (그래야 같은 어셈블리가 두 번 정의되는 충돌 없이 편집 가능)

## 설치 & Import

**1) 패키지 추가** — Unity → Window → Package Manager → **+** → *Add package from git URL…*:

```
https://github.com/BorderJung/unity-modules.git#v2.0.0
```

**2) Import** — Package Manager에서 **Border Unity Modules** 선택 → **Samples** 탭 → **Border Modules (full source)** 의 **Import** 클릭.

→ `Assets/Samples/Border Unity Modules/<버전>/Border Modules (full source)/{Runtime, Editor, Demo}` 에 전체 소스가 **편집 가능**하게 들어옵니다 (GUID 보존 → 프리팹/SO 연결 유지). 이후엔 프로젝트 소유 코드로 자유롭게 수정하세요.

### 업데이트 (버전 올릴 때)

1. `Packages/manifest.json`의 URL 태그를 새 버전으로 (`#vX.Y.Z`) 바꾼다.
2. Package Manager → Samples → **다시 Import**.
3. ⚠️ **이전 버전 Import 폴더(`Assets/Samples/.../<옛버전>/`)를 삭제**한다. 안 지우면 같은 `Border` 어셈블리가 두 번 존재해 컴파일 충돌이 난다.

## 포함된 모듈

| 영역 | 네임스페이스 | 내용 |
|---|---|---|
| Core | `Border.Core` | 빌드에서 자동 제거되는 조건부 `Log`, 결정론적 xorshift32 RNG(`DeterministicRng`), 스크린샷 헬퍼(`ScreenshotManager`) |
| Events | `Border.Events` | ScriptableObject 이벤트 채널 (Void/Bool/Int/Float/Vector2/String) + Fade 채널. 인스펙터에서 연결하는 옵저버 패턴 |
| SaveLoad | `Border.SaveLoad` | JSON 저장/로드 (`SaveLoadSystem`, `FileManager`, `Save`) |
| Localization | `Border.Localization` | SO 테이블 + `LocalizationManager`(`ILocalizationProvider`, 정적 `Current`), `UILocalizeText`, `[LocalizeKey]` 에디터 |
| Settings | `Border.Settings` | SO 기반 설정(`SettingsSystem`/`SettingsSO`) + `ISettingsRepository` 주입, 그래픽/오디오 UI |
| UI | `Border.UI` | `UIGenericButton`, 선택 프레임 토글(`UISelectionFrameHook`), 보조 그래픽 색상 동기화(`SelectableExtraGraphics`) 등 공용 UI. 데모 프리팹은 Samples에 포함 |
| Input | `Border.Input` | Input System 콜백 → `UnityAction` 이벤트 브리지(`InputReader`) + 생성 클래스 `GameInput`(namespace `Border.Input`). 전용 asmdef + `com.unity.inputsystem` 게이팅 |
| Pool | `Border.Pool` | 제네릭 오브젝트 풀 프레임워크 — `IPool<T>`/`IFactory<T>`, `FactorySO<T>`, `PoolSO<T>`, `ComponentPoolSO<T>`, 씬 언로드 자동 정리 `PoolRegistry` |
| Audio | `Border.Audio` | SO/이벤트채널 기반 오디오 재생 틀 — `AudioManager`(풀에서 `SoundEmitter` 재생 + `AudioMixer` 볼륨), 재생 채널 `AudioCueEventChannelSO`, `AudioCueSO`/`AudioConfigurationSO`, `AudioRegistrySO`(AudioId 조회). 매핑 데이터 미포함 |

런타임 코드는 단일 어셈블리 `Border`, 에디터 코드는 `Border.Editor`로 컴파일됩니다.
`using Border.Core;` / `using Border.Events;` / `using Border.Settings;` 등으로 접근합니다.

## 사용 예

```csharp
using Border.Core;

Log.D("hello");            // UNITY_EDITOR에서만 출력, 빌드에서 자동 strip
var rng = new DeterministicRng(seed: 12345);
int roll = rng.NextInt(0, 6);
```

## 라이선스

MIT — [LICENSE.md](LICENSE.md)

---

> 이 repo는 단일 패키지로 배포되지만, 패키지가 아닌 개발 자료(데모 프로젝트 설정,
> 추출 로드맵, 아직 패키지화 전인 모듈들)는 Unity가 무시하는 `Dev~/` 폴더에 보관됩니다.
