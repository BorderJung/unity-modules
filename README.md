# Border Unity Modules

`com.borderjung.unity-modules` — BorderJung의 재사용 Unity 모듈을 **하나의 UPM 패키지**로 묶은 것.
git URL 한 줄로 어떤 프로젝트에든 바로 드롭인할 수 있게 만든 라이브러리입니다.

- **Package id**: `com.borderjung.unity-modules`
- **Assembly / root namespace**: `Border`
- **Min Unity**: 2021.3
- **외부 의존성**: `com.unity.ugui` (TextMeshPro/UGUI) — Localization/Settings/UI가 사용

## 설치 (Package Manager git URL)

Unity → Window → Package Manager → **+** → *Add package from git URL…* 에 아래를 입력:

```
https://github.com/BorderJung/unity-modules.git
```

또는 프로젝트의 `Packages/manifest.json`에 직접:

```json
{
  "dependencies": {
    "com.borderjung.unity-modules": "https://github.com/BorderJung/unity-modules.git"
  }
}
```

> 버전을 고정하려면 끝에 git 태그를 붙입니다: `...unity-modules.git#v1.0.0`
> 태그 없이 받으면 항상 `main` 최신을 가져오므로 빌드가 흔들릴 수 있습니다.

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
