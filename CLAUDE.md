# 픽셀 던전 클론 - Unity 2D 로그라이크 프로젝트

## 프로젝트 개요

- **게임명**: Dungeon (픽셀 던전 클론)
- **장르**: 2D 도트 로그라이크 RPG
- **엔진**: Unity 2022 LTS (Universal 2D)
- **플랫폼**: 모바일 (iOS/Android)
- **참고 게임**: Pixel Dungeon

## 기술 스택

- Unity 2022 LTS (Universal 2D)
- C# (.NET Standard 2.1)
- Unity Input System (터치 입력)
- Unity 2D Tilemap System
- Unity Animation 2D

## 설치된 유니티 패키지

- 2D Animation - 9.2.0
- 2D Aseprite Importer - 1.1.9
- 2D Common - 8.1.0
- 2D Pixel Perfect - 5.1.0
- 2D PSD Importer - 8.1.0
- 2D Sprite - 1.0.0
- 2D SpriteShape - 9.1.0
- 2D Tilemap Editor - 1.0.0
- 2D Tilemap Extras - 3.1.3
- Addressables - 1.22.3
- Burst - 1.8.21
- Collections - 1.2.4
- Core RP Library - 14.0.12
- Custom NUnit - 1.0.6
- Device Simulator Devices - 1.0.0
- Input System - 1.14.2
- JetBrains Rider Editor - 3.0.36
- Mathematics - 1.2.6
- Newtonsoft Json - 3.2.1
- Scriptable Build Pipeline - 1.21.25
- Searcher - 4.9.2
- Shader Graph - 14.0.12
- Test Framework - 1.1.33
- TextMeshPro - 3.0.9
- Timeline - 1.7.7
- Unity UI - 1.0.0
- Unity Version Control - 2.9.2
- Universal RP - 14.0.12
- Universal RP Config - 14.0.10
- Visual Scripting - 1.9.4
- Visual Studio Editor - 2.0.22

## MCP 도구 사용 가이드

### 1. sequential-thinking

복잡한 게임 메커니즘 설계나 알고리즘 구현 시 사용:

- 던전 생성 알고리즘 설계
- 전투 시스템 로직 구현
- 아이템 드롭 확률 계산

### 2. context7

Unity 및 관련 라이브러리 문서 참조:

- Unity 2D 관련 API 문서
- C# 최신 문법 및 패턴
- 모바일 최적화 가이드

### 3. Unity MCP (읽기 전용으로 사용)

Unity 프로젝트 상태 확인 및 C# 스크립트 작업:

- C# 스크립트 생성/수정 (코드 작업만)
- Unity 상태 확인 (읽기 전용)
  - Scene 구조 확인
  - GameObject 컴포넌트 검사
  - 에러/경고 확인
  
**중요**: Unity Editor 내의 GameObject, Component, Asset 수정은 불가능합니다.
Unity Editor 설정이 필요한 경우 상세한 가이드를 제공하니 수동으로 작업해주세요.

## 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Player/          # 플레이어 관련 스크립트
│   ├── Enemies/         # 적 AI 및 행동
│   ├── Items/           # 아이템 시스템
│   ├── Dungeon/         # 던전 생성 및 관리
│   ├── UI/              # UI 컨트롤러
│   ├── Combat/          # 전투 시스템
│   └── Managers/        # 게임 매니저
├── Prefabs/             # 프리팹
├── Sprites/             # 2D 스프라이트 및 애니메이션
├── Tilemaps/            # 타일맵 에셋
├── Materials/           # 머티리얼 및 셰이더
├── Audio/               # 사운드 및 음악
└── Scenes/              # 게임 씬
```

## 핵심 게임 시스템

### 1. 던전 생성 시스템

- **알고리즘**: BSP (Binary Space Partitioning) 또는 Cellular Automata
- **특징**:
  - 랜덤하게 생성되는 방과 복도
  - 층별로 난이도 증가
  - 특별한 방 (상점, 보스룸, 보물방)

### 2. 전투 시스템

- **턴제 전투**: 플레이어가 움직일 때마다 적도 움직임
- **데미지 계산**: 공격력 - 방어력 기본 공식
- **크리티컬 및 회피**: 확률 기반 시스템

### 3. 아이템 시스템

- **장비**: 무기, 방어구, 반지, 아티팩트
- **소비 아이템**: 포션, 스크롤, 음식
- **인벤토리**: 제한된 슬롯 시스템
- **아이템 강화**: 강화 스크롤 사용

### 4. 캐릭터 클래스

- 전사 (Warrior): 높은 체력과 방어력
- 도적 (Rogue): 높은 회피율과 크리티컬
- 마법사 (Mage): 지팡이 충전 시스템
- 사냥꾼 (Huntsman): 원거리 공격 특화

### 5. UI/UX

- **터치 컨트롤**: 탭하여 이동, 길게 눌러 조사
- **가상 조이스틱**: 선택적 컨트롤
- **미니맵**: 탐험한 지역 표시
- **퀵슬롯**: 자주 사용하는 아이템 배치

## 코딩 규칙

### 네이밍 컨벤션

```csharp
// 클래스명: PascalCase
public class PlayerController { }

// 메서드명: PascalCase
public void TakeDamage(int damage) { }

// 변수명: camelCase
private int currentHealth;

// 상수: UPPER_SNAKE_CASE
private const int MAX_INVENTORY_SIZE = 20;

// Unity 컴포넌트 참조: _로 시작
[SerializeField] private Rigidbody2D _rigidbody;
```

### 스크립트 템플릿

```csharp
using UnityEngine;

namespace Dungeon.{Category}
{
    public class {ClassName} : MonoBehaviour
    {
        #region Fields
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        private int _currentValue;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // 초기화
        }

        private void Start()
        {
            // 게임 시작 시 설정
        }
        #endregion

        #region Public Methods
        public void PublicMethod()
        {
            // 공개 메서드
        }
        #endregion

        #region Private Methods
        private void PrivateMethod()
        {
            // 비공개 메서드
        }
        #endregion
    }
}
```

## 성능 최적화 가이드

### 모바일 최적화

- **드로우 콜 최소화**: 스프라이트 아틀라스 사용
- **오브젝트 풀링**: 적, 투사체, 이펙트에 적용
- **LOD**: 거리에 따른 디테일 조절
- **텍스처 압축**: ETC2 (Android), PVRTC (iOS)

### 메모리 관리

- **리소스 로딩**: Resources.Load 대신 Addressables 사용 고려
- **가비지 컬렉션**: 문자열 연결 최소화, StringBuilder 사용
- **씬 관리**: 불필요한 오브젝트 적시 해제

## 빌드 및 테스트

### 빌드 설정

```bash
# Android 빌드
Unity Editor > File > Build Settings > Android
- Texture Compression: ETC2
- Target API Level: 30+
- Scripting Backend: IL2CPP

# iOS 빌드
Unity Editor > File > Build Settings > iOS
- Target SDK: iOS 14.0+
- Architecture: ARM64
```

### 테스트 체크리스트

- [ ] 터치 입력 반응성
- [ ] 다양한 화면 비율 대응
- [ ] 메모리 사용량 모니터링
- [ ] 배터리 소모 확인
- [ ] 네트워크 오프라인 상태 처리

## 자주 사용하는 Unity 명령

### 에디터 명령

- **Play Mode**: Ctrl+P (테스트 실행)
- **Scene Save**: Ctrl+S
- **Prefab Apply**: Ctrl+Shift+S

### 코드 컴파일 확인

```bash
# Unity 콘솔에서 컴파일 에러 확인
# Window > General > Console
```

## 참고 자료

- [Unity 2D 로그라이크 튜토리얼](https://learn.unity.com/project/2d-roguelike-tutorial)
- [Pixel Dungeon 오픈소스](https://github.com/watabou/pixel-dungeon)
- [Unity 모바일 최적화 가이드](https://docs.unity3d.com/Manual/MobileOptimization.html)

## Unity Editor 작업 프로세스

### Unity 설정이 필요한 경우:
1. **코드 작성**: 필요한 모든 C# 스크립트를 먼저 생성
2. **가이드 제공**: Unity Editor에서 수행할 단계별 설정 방법 제공
3. **수동 작업**: 개발자가 Unity Editor에서 직접 설정
4. **검증**: 필요시 Unity MCP로 설정 상태 확인 (읽기 전용)

### 예시:
- GameObject 생성 → 가이드 제공 → 수동 생성
- Component 추가 → 가이드 제공 → 수동 추가
- Prefab 생성 → 가이드 제공 → 수동 생성
- Inspector 설정 → 가이드 제공 → 수동 설정

## 추가 개발 노트

- 세이브 시스템: PlayerPrefs 또는 JSON 파일 사용
- 업적 시스템: Google Play Games / Game Center 연동
- 광고 통합: Unity Ads 또는 AdMob
- 분석: Unity Analytics 또는 Firebase

---

_이 문서는 프로젝트 진행에 따라 지속적으로 업데이트됩니다._
