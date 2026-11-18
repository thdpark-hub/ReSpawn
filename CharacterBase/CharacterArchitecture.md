# 캐릭터 더미(Base) 구조

전제: Unity 6000.4 · 3D 휴머노이드 · Cinemachine 3.1 · **FishNet 4** (호스트/클라이언트/전용 서버 모두 지원) · CharacterController · Input System · 캐릭터 4명.

스크립트는 `Assets/_Game/Scripts/` 에 있다. 프로젝트의 `Assets/` 로 그대로 복사하면 된다.
22개 파일 모두 Unity 6000.4.3 + FishNet + Cinemachine 3.1.7 + TMP DLL 기준으로 **컴파일 오류 0** 확인.

스켈레톤 규칙
- 로직 메서드는 본문이 비어 있다. 주석 `호출:` = 누가 부르나, `구현:` = 무엇을 넣을까.
- 값을 반환하는 메서드는 컴파일용으로 `return default;` 만 있다 (구현 전엔 false/0/null).
- 참조를 그대로 넘기는 한 줄 프로퍼티(`Health => health`, `Current => _current.Value`)는 로직이 아니라서 채워 뒀다.
- **완성본(본문 있음)**: `LocalPlayerBinder`, `PlayerCameraRig`, `PlayerHUD`, `SkillSlotView`, `CharacterNameplate` (카메라·UI 할당).
- 구현 전까지 "사용되지 않는 필드/이벤트" 경고(CS0414, CS0067)가 뜬다. 구현하면 사라진다.
- 네트워크 흐름 위치는 코드에 `[흐름①]`~`[흐름⑥]` 로 표시돼 있다 (검색용).

---

## 1. 하이어라키

아래 구조는 이미 `PF_Character_Base`, `PF_PlayerFollowCamera`, `PF_PlayerHUD` 프리팹으로 만들어져 있고 참조도 모두 연결돼 있다.
레이어 `Character`(8), `Hurtbox`(10)이 추가됐고, GroundCheck 마스크는 두 레이어를 제외, MeleeHitbox 마스크는 Hurtbox 만.
씬 쪽은 Main Camera 에 `CinemachineBrain` 을 붙이고 두 씬용 프리팹을 끌어다 놓으면 된다.

표기: **(a)** 역할 · **(b)** 컴포넌트 · **(c)** 캐릭터별 변경 여부

```
PF_Character_Base  (Root, Layer = Character)
│ (a) 네트워크 단위, 물리 본체, 공통 로직 허브
│ (b) NetworkObject
│     NetworkTransform            [TODO: 결정 필요] 이동 권한 모델 (1)이면 Client Authoritative, (2) 예측이면 제거
│     (NetworkAnimator)           [TODO: 결정 필요] 원격 애니 동기화 방식 (a)일 때만
│     CharacterController
│     CharacterCore, CharacterMovement, CharacterHealth, CharacterNetworkSync,
│     CharacterVisualLoader, CharacterAnimatorBridge, AbilityController,
│     CharacterInput (enabled = false), LocalPlayerBinder
│ (c) 부분 — Variant 에서 Core.definition 교체, CharacterController height/radius/center 조정
│
├── Visual
│   │ (a) 외형 슬롯. 자신은 빈 Transform (모델 피벗 보정 오프셋 용도)
│   │ (b) 없음
│   │ (c) 예 — 자식 모델이 캐릭터마다 다름
│   └── PF_Model_CharA            ← 접근 A: Variant 에서 넣어 둠 / 접근 B: 런타임 Instantiate
│       │ (b) Animator (Controller = AOC_CharA, Avatar = 해당 모델 Avatar)
│       │     AnimatorEventRelay  ← Bridge.Bind 가 런타임에 자동 추가 (미리 붙일 필요 없음)
│       └── Armature/…/RightHand
│                       └── Socket_RightHand   본 소켓 마커 (모델 프리팹 안, 캐릭터별 위치)
│
├── Abilities
│   │ (a) 특수 기능 컨테이너. AbilityController 가 이 아래 CharacterAbility 를 수집
│   │ (b) 없음 (자식들이 가짐)
│   │ (c) 예 — 자식 구성이 캐릭터마다 다름
│   ├── Ability_Skill1     (DashAbility,   Slot = Skill1)     ← Variant 에서 추가
│   ├── Ability_Skill2     (…,             Slot = Skill2)
│   └── Ability_Ultimate   (…,             Slot = Ultimate)
│
├── Sockets
│   │ (a) 루트 기준 소켓. 애니메이션과 무관 → 서버 판정 기준으로 안전
│   │ (b) 없음 (빈 Transform, 이름 = SocketId)
│   │ (c) 부분 — 위치만 Variant 에서 조정
│   ├── Center             가슴 높이 (범위기·조준 보정 중심)
│   ├── Overhead           머리 위 (버프 아이콘·이펙트)
│   ├── ProjectileOrigin   발사 원점 (서버 판정 기준)
│   └── Feet               바닥 범위기 중심
│
├── GroundCheck            (a) 지면 검사 구 중심, 캡슐 바닥 살짝 아래  (b) 없음  (c) 부분 — 캡슐 크기에 맞춰 y
├── CameraTarget           (a) Cinemachine Follow/LookAt 대상, 어깨~머리 높이  (b) 없음  (c) 부분 — 키에 맞춰 y
│
├── Combat
│   ├── Hurtbox            (a) 피격 영역  (b) CapsuleCollider(isTrigger), Hurtbox, Layer = Hurtbox  (c) 부분 — 크기
│   └── Hitboxes           (a) 근접 판정 볼륨 모음, 루트 기준  (c) 예
│       └── MeleeHitbox    (b) Hitbox (콜라이더 없음, OverlapBox 쿼리)
│
└── NameplateAnchor        (a) 이름표 위치  (b) 없음  (c) 부분 — 키에 맞춰 y
    └── Nameplate          (b) Canvas(World Space, scale 0.01), CharacterNameplate  (c) 아니오
        ├── Name           TMP_Text
        └── HealthBar/Fill Image (Filled, Horizontal)
```

**소켓: 본에 붙일까, 루트에 둘까**
- **루트(`Sockets`)**: 발사 원점, 범위기 중심 등 *서버가 판정에 쓰는 위치*. 전용 서버는 Animator 를 끄거나 컬링할 수 있어 본 위치를 믿을 수 없다.
- **본(`Socket_*` 마커, 모델 프리팹 안)**: 무기 쥐기, 손 이펙트, 머즐 플래시 등 *연출*. 애니메이션을 따라가야 자연스럽다.
- `CharacterVisualLoader.GetSocket(id)` 가 본 마커 → 루트 소켓 → 루트 순으로 찾아 준다. 모델 Rig 에서 *Optimize Game Objects* 를 켜면 본이 숨겨지므로 마커가 달린 본을 *Extra Transforms to Expose* 에 넣을 것.

**씬 쪽 오브젝트 (프리팹 밖, 씬마다 1개)**

```
Scene
├── NetworkManager                (FishNet)
├── Main Camera                   Camera, CinemachineBrain, AudioListener
├── PlayerFollowCamera            CinemachineCamera, CinemachineOrbitalFollow, CinemachineRotationComposer,
│                                 CinemachineInputAxisController, CinemachineDeoccluder, PlayerCameraRig
│                                 (3인칭 자유 궤도. Deoccluder 는 Character/Hurtbox/UI/Ignore Raycast 를 빼고 충돌 → 벽 앞으로 당김)
└── HUDCanvas                     Canvas(Screen Space), PlayerHUD     ← 이 오브젝트는 항상 켜 둠
    └── Content                   (기본 비활성, PlayerHUD.content)
        ├── Portrait (Image), Name (TMP_Text)
        ├── HealthBar/Fill (Image Filled), HealthText (TMP_Text)
        └── SkillSlots
            ├── Skill1    SkillSlotView → Icon(Image), CooldownMask(Image Filled Radial360), CooldownText(TMP_Text)
            ├── Skill2    SkillSlotView
            └── Ultimate  SkillSlotView
```

카메라·HUD·입력 할당 순서: 스폰 → `OnStartNetwork`(CharacterCore 가 전부 초기화) → `OnStartClient` → `OnOwnershipClient` → **LocalPlayerBinder** 가 `IsOwner` 일 때만 `PlayerCameraRig.SetTarget(CameraTarget)`, `PlayerHUD.Bind(core)`, `CharacterInput.enabled = true`. 소유권을 잃거나 디스폰되면 반대로 해제. (FishNet 이 모든 behaviour 의 `OnStartNetwork` 를 끝낸 뒤 `OnStartClient`/`OnOwnershipClient` 를 부르는 것을 소스에서 확인함.)

---

## 2. 폴더 구조

```
Assets/_Game/
├── Scripts/
│   ├── Characters/
│   │   ├── Core/        CharacterCore, CharacterMovement, CharacterHealth, CharacterNetworkSync,
│   │   │                CharacterInput, LocalPlayerBinder
│   │   ├── Visual/      CharacterVisualLoader, CharacterAnimatorBridge, AnimatorEventRelay
│   │   ├── Combat/      Hurtbox, Hitbox
│   │   ├── Abilities/   CharacterAbility, AbilityController
│   │   │   └── PerCharacter/  DashAbility, ShieldAbility, ProjectileAbility, HealPulseAbility
│   │   └── Data/        CharacterDefinition
│   ├── CameraSystem/    PlayerCameraRig
│   └── UI/              PlayerHUD, SkillSlotView, CharacterNameplate
├── Data/Characters/     CD_CharA.asset … CD_CharD.asset
├── Prefabs/
│   ├── Characters/      PF_Character_Base.prefab (생성됨), PF_Char_A.prefab (Variant) … PF_Char_D.prefab
│   ├── Camera/          PF_PlayerFollowCamera.prefab (생성됨) — 씬에 1개
│   ├── UI/              PF_PlayerHUD.prefab (생성됨) — 씬에 1개
│   └── Models/          PF_Model_CharA.prefab … (FBX 를 감싼 모델 프리팹 + Socket_* 마커)
├── Animations/
│   ├── Base/            AC_Character_Base.controller
│   │   └── Placeholders/  __Idle.anim, __Skill1_Start.anim … (슬롯별 고유 클립)
│   ├── Masks/           AM_UpperBody.mask
│   └── CharA/ … CharD/  AOC_CharA.overrideController + 클립
├── Models/CharA/ … CharD/   FBX, 텍스처, 머티리얼
└── Input/               InputSystem_Actions (Skill1/Skill2/Ultimate 액션 추가)
```

---

## 3. 스크립트

| 파일 | 타입 | 역할 |
|---|---|---|
| [CharacterDefinition.cs](Assets/_Game/Scripts/Characters/Data/CharacterDefinition.cs) | ScriptableObject | ID·이름·모델·AOC·Avatar·스탯·아이콘·능력 메타·네트워크 프리팹. `CharacterStats`, `AbilityInfo` 포함 |
| [CharacterCore.cs](Assets/_Game/Scripts/Characters/Core/CharacterCore.cs) | NetworkBehaviour | 참조 허브 + 초기화 순서 (`Awake` 로컬 준비 → `OnStartNetwork` 네트워크 초기화) |
| [CharacterMovement.cs](Assets/_Game/Scripts/Characters/Core/CharacterMovement.cs) | NetworkBehaviour | CharacterController 이동·점프·중력·외부 힘·루트모션. `MoveInput` 포함 |
| [CharacterHealth.cs](Assets/_Game/Scripts/Characters/Core/CharacterHealth.cs) | NetworkBehaviour | 서버 권한 체력 SyncVar. `DamageInfo`, 확장 지점 `IDamageModifier` 포함 |
| [CharacterNetworkSync.cs](Assets/_Game/Scripts/Characters/Core/CharacterNetworkSync.cs) | NetworkBehaviour | 플레이어 이름·characterId·팀 SyncVar |
| [CharacterInput.cs](Assets/_Game/Scripts/Characters/Core/CharacterInput.cs) | MonoBehaviour | 소유자 전용 입력 → Movement / AbilityController ([흐름①]) |
| [LocalPlayerBinder.cs](Assets/_Game/Scripts/Characters/Core/LocalPlayerBinder.cs) | NetworkBehaviour | **완성본.** 소유자일 때 카메라·HUD·입력 연결/해제 |
| [CharacterVisualLoader.cs](Assets/_Game/Scripts/Characters/Visual/CharacterVisualLoader.cs) | MonoBehaviour | 모델 생성/교체, Animator 설정, 소켓 연결. `SocketId` 포함 |
| [CharacterAnimatorBridge.cs](Assets/_Game/Scripts/Characters/Visual/CharacterAnimatorBridge.cs) | MonoBehaviour | 로직 → Animator 통로, 루트모션/이벤트 수신. `AnimParams`, `AnimEvents` 상수 |
| [AnimatorEventRelay.cs](Assets/_Game/Scripts/Characters/Visual/AnimatorEventRelay.cs) | MonoBehaviour | 모델 루트에서 `OnAnimatorMove`·애니 이벤트를 Bridge 로 중계 |
| [Hurtbox.cs](Assets/_Game/Scripts/Characters/Combat/Hurtbox.cs) / [Hitbox.cs](Assets/_Game/Scripts/Characters/Combat/Hitbox.cs) | MonoBehaviour | 피격 영역 / 서버 근접 판정 |
| [CharacterAbility.cs](Assets/_Game/Scripts/Characters/Abilities/CharacterAbility.cs) | abstract MonoBehaviour | 특수 기능 베이스. 실행 위치별 virtual/abstract. `AbilityContext` 포함 |
| [AbilityController.cs](Assets/_Game/Scripts/Characters/Abilities/AbilityController.cs) | NetworkBehaviour | 능력 수집·슬롯 매핑 + 발동 RPC 흐름 전부. `AbilitySlot` 포함 |
| [DashAbility](Assets/_Game/Scripts/Characters/Abilities/PerCharacter/DashAbility.cs) · [ShieldAbility](Assets/_Game/Scripts/Characters/Abilities/PerCharacter/ShieldAbility.cs) · [ProjectileAbility](Assets/_Game/Scripts/Characters/Abilities/PerCharacter/ProjectileAbility.cs) · [HealPulseAbility](Assets/_Game/Scripts/Characters/Abilities/PerCharacter/HealPulseAbility.cs) | CharacterAbility | [캐릭터A]~[캐릭터D] 예시 틀 |
| [PlayerCameraRig.cs](Assets/_Game/Scripts/CameraSystem/PlayerCameraRig.cs) | MonoBehaviour | **완성본.** 씬 Cinemachine 카메라에 대상 할당/해제 |
| [PlayerHUD.cs](Assets/_Game/Scripts/UI/PlayerHUD.cs) / [SkillSlotView.cs](Assets/_Game/Scripts/UI/SkillSlotView.cs) | MonoBehaviour | **완성본.** 로컬 플레이어 체력·초상화·스킬 쿨다운 |
| [CharacterNameplate.cs](Assets/_Game/Scripts/UI/CharacterNameplate.cs) | MonoBehaviour | **완성본.** 머리 위 이름·체력바 빌보드, 자기 캐릭터는 숨김 |

설계 결정 두 가지:
- **능력은 MonoBehaviour, RPC 는 AbilityController 한 곳.** 능력 작성자는 "서버에서 뭘 하고 클라에서 뭘 보여 줄지"만 오버라이드한다. 서버 검증을 빠뜨릴 수 없고, 런타임에 능력을 붙여도 동작한다 (FishNet 은 스폰 후 NetworkBehaviour 를 추가할 수 없음).
- **공통 코드 수정 없는 확장 지점**: 새 능력 = `CharacterAbility` 상속 컴포넌트 추가. 데미지에 끼어드는 능력(실드·무적) = `IDamageModifier` 구현 → `CharacterHealth` 가 자동 수집.

---

## 4. 애니메이션

### 4-1. 공통 파라미터 (AC_Character_Base, 코드 상수 = `AnimParams`)

| 파라미터 | 타입 | 기본값 | 누가 설정 | 용도 |
|---|---|---|---|---|
| MoveSpeed | Float | 0 | Movement → `Bridge.SetLocomotion` | 수평 속도 0~1 (걷기 ≈0.5, 달리기 1). Locomotion 블렌드 |
| MoveX | Float | 0 | 〃 | 로컬 좌우 (스트레이프 블렌드) |
| MoveY | Float | 0 | 〃 | 로컬 전후 |
| IsGrounded | Bool | true | 〃 | 공중/착지 판정 |
| VerticalSpeed | Float | 0 | 〃 | 상승/낙하 블렌드 |
| Jump | Trigger | – | Movement → `Bridge.TriggerJump` | 점프 시작 |
| Skill | Trigger | – | AbilityController [흐름⑤] → `Bridge.PlaySkill` | 스킬 진입 |
| SkillIndex | Int | 0 | 〃 | 1 = Skill1, 2 = Skill2, 3 = Ultimate (0 = 기본 공격용 예약) |
| IsChanneling | Bool | false | 능력 → `Bridge.SetChanneling` | `Loop` 상태 유지 |
| Hit | Trigger | – | Health [ObserversDamaged] → `Bridge.PlayHit` | 피격 리액션 (UpperBody) |
| IsDead | Bool | false | Health [IsDead SyncVar] → `Bridge.SetDead` | 사망/부활 |
| ActionSpeed | Float | 1 | 능력 → `Bridge.SetActionSpeed` | 스킬 상태 Speed Multiplier (공속 버프) |

### 4-2. 레이어와 상태 (이름은 모든 캐릭터 동일)

| 레이어 | 상태 | 플레이스홀더 클립 | 들어가는 조건 | 나가는 조건 |
|---|---|---|---|---|
| Base (전신) | **Locomotion** (2D Freeform Directional, MoveX/MoveY) | `__Idle`, `__WalkF/B/L/R`, `__RunF` | 기본 상태 | – |
| | **Jump_Start** | `__JumpStart` | `Jump` | Exit Time → Jump_Air |
| | **Jump_Air** (루프) | `__Fall` | Jump_Start 종료, 또는 Locomotion 에서 `IsGrounded = false` | `IsGrounded = true` → Jump_Land |
| | **Jump_Land** | `__Land` | – | Exit Time → Locomotion |
| | **Skill1** 서브 스테이트머신: `Start` / `Loop` / `End` | `__Skill1_Start`, `__Skill1_Loop`, `__Skill1_End` | Any State: `Skill` + `SkillIndex == 1` + `IsDead == false` | Start → (IsChanneling ? Loop : Exit), Loop → End (`IsChanneling = false`) → Exit |
| | **Skill2**, **Skill3**(Ultimate) | `__Skill2_*`, `__Skill3_*` | 같은 방식, SkillIndex 2 / 3 | 같음 |
| | **Death** | `__Death` | Any State: `IsDead = true` | `IsDead = false` → Locomotion (부활) |
| UpperBody (Avatar Mask = AM_UpperBody, Override) | **Empty** | (모션 없음) | 기본 | – |
| | **HitReact** | `__Hit` | Any State: `Hit` | Exit Time → Empty |

- Any State → Skill/Death 전환은 *Can Transition To Self* 끄기.
- 스킬 상태 Speed 의 *Multiplier* 에 `ActionSpeed` 연결.
- [TODO: 결정 필요] 스트레이프가 없으면 Locomotion 을 1D(MoveSpeed: Idle/Walk/Run)로 단순화. 이동하며 스킬을 쓰면 Skill 상태를 UpperBody 레이어로 옮긴다.

### 4-3. 캐릭터별 AnimatorOverrideController

1. `Create > Animator Override Controller` → `AOC_CharA`, *Controller* = `AC_Character_Base`.
2. 목록에 나오는 `__플레이스홀더` 마다 캐릭터 클립을 넣는다. 상태/전환/파라미터는 베이스 것을 그대로 쓴다.
3. `CD_CharA.animatorOverride` 에 지정 → `CharacterVisualLoader.SetupAnimator` 가 `runtimeAnimatorController` 로 넣는다.

주의
- **플레이스홀더는 상태마다 서로 다른 클립이어야 한다.** 오버라이드는 클립 참조 단위라 두 상태가 같은 클립을 쓰면 함께 바뀐다.
- 플레이스홀더는 빈 클립 말고 Idle 복사본(1프레임)으로. 캐릭터가 슬롯을 안 채우면 빈 클립은 이상한 기본 포즈가 나온다.
- 베이스에 상태를 추가하면 4개 AOC 에 새 칸이 생긴다 → 체크리스트로 4개 모두 채웠는지 확인.

### 4-4. 캐릭터 고유 특수 모션

1. **범용 슬롯 (기본)**: Skill1~3 × Start/Loop/End 9칸을 미리 만들어 두고 오버라이드. 대부분의 고유 모션은 여기에 들어간다.
2. **슬롯이 모자라면**: 베이스에 `Special1`, `Special2` 같은 범용 상태를 미리 더 만든다 (SkillIndex 4, 5). 상태 구조 변경은 베이스에서만.
3. **임의 클립을 코드에서 재생해야 하면**: 인스턴스 전용 AOC 를 복제해 슬롯 클립을 런타임에 바꾼다. `new AnimatorOverrideController(def.AnimatorOverride.runtimeAnimatorController)` 후 원본의 `GetOverrides` → `ApplyOverrides`, 그다음 `aoc["__Special"] = clip`. 공유 자산을 직접 고치면 같은 캐릭터 전원이 바뀐다.
4. **구조 자체가 다른 캐릭터**(비행, 탈것 등)는 공통 컨트롤러로 억지로 맞추지 말고 별도 베이스 컨트롤러 [TODO: 결정 필요].

### 4-5. Avatar 리타게팅 주의사항 (3D 휴머노이드)

- 모델 FBX: *Rig > Animation Type = Humanoid*, *Avatar Definition = Create From This Model*. *Configure* 에서 필수 본 전부 초록, T-포즈 맞추기.
- 애니메이션 전용 FBX(Mixamo 등)도 Humanoid. 같은 리그면 *Copy From Other Avatar*.
- 제자리 이동 클립 사용: *Root Transform Rotation / Position(Y) / Position(XZ)* 모두 *Bake Into Pose*. 이동은 CharacterController 가 한다.
- 루프 클립은 *Loop Time* + *Loop Pose*, 루프 매치 표시가 초록인지 확인.
- 휴머노이드 루트모션 거리는 아바타 크기(humanScale)에 비례한다 → 루트모션으로 게임플레이 거리를 정하면 캐릭터마다 달라진다. 거리는 코드/데이터로.
- 체형이 다르면 같은 클립에서도 손 위치가 달라진다 → 무기는 본 소켓 마커 위치로 캐릭터별 보정, 이동 상태엔 *Foot IK* 켜서 발 미끄러짐 완화.
- 휴머노이드 클립은 꼬리·망토·머리카락 같은 비휴머노이드 본을 움직이지 않는다 → 물리 본 / Animation Rigging 으로.
- 손가락 머슬이 없는 아바타는 손 모양이 클립과 다르게 나온다 (주먹/펼침).
- Avatar Mask(UpperBody)는 휴머노이드 부위 기준이라 4명이 하나를 공유할 수 있다.
- 런타임에 avatar/컨트롤러를 바꾸면 `Animator.Rebind()` (Loader 주석에 포함).
- *Optimize Game Objects* 를 켜면 본이 숨겨진다 → 소켓 마커 본은 *Extra Transforms to Expose* 에.

---

## 5. 새 캐릭터 제작 체크리스트

**0. 모델 준비**
- [ ] FBX 임포트: Humanoid, Avatar 생성, Configure 전부 초록
- [ ] 클립 임포트: Humanoid, Bake Into Pose, Loop 설정
- [ ] 모델 프리팹 `PF_Model_CharX` 생성 (FBX 를 감쌈): 루트에 Animator (Controller 는 비워도 됨), 딸려 온 Collider/Rigidbody 제거
- [ ] 본 아래 `Socket_RightHand`, `Socket_LeftHand`, `Socket_Head` 빈 오브젝트 추가

**1. Prefab Variant 생성**
- [ ] `PF_Character_Base` 우클릭 > *Create > Prefab Variant* → `PF_Char_X`

**2. 모델 넣기**
- [ ] Variant 를 열고 `Visual` 아래에 `PF_Model_CharX` 배치 (local position/rotation 0)
- [ ] CharacterController height/radius/center, `Combat/Hurtbox` 캡슐을 모델에 맞춤
- [ ] `GroundCheck`, `CameraTarget`, `NameplateAnchor`, `Sockets/*` 높이 조정
- [ ] `Combat/Hitboxes/MeleeHitbox` 크기를 무기 길이에 맞춤

**3. Override Controller 연결**
- [ ] `AOC_CharX` 생성, Controller = `AC_Character_Base`
- [ ] 모든 `__플레이스홀더` 에 클립 지정 (빈 칸 없음)

**4. Definition 생성**
- [ ] `Create > Game/Character Definition` → `CD_CharX`
- [ ] characterId(고유), 이름, modelPrefab, animatorOverride, avatar, 아이콘, 스탯
- [ ] abilities: 슬롯마다 이름·아이콘·쿨다운
- [ ] networkPrefab = `PF_Char_X`
- [ ] `PF_Char_X` 의 CharacterCore.definition = `CD_CharX`

**5. 특수 기능 컴포넌트 추가**
- [ ] `Abilities` 아래 자식 `Ability_Skill1` 생성 → 능력 컴포넌트 추가, Slot 지정 (Skill2, Ultimate 반복)
- [ ] 새 능력이면 `CharacterAbility` 상속 스크립트 작성 (`PerCharacter/` 폴더). 공통 스크립트는 건드리지 않는다
- [ ] Definition 의 AbilityInfo 슬롯과 컴포넌트 Slot 이 1:1 인지 확인

**6. 네트워크 프리팹 등록**
- [ ] NetworkManager 의 Spawnable Prefabs (DefaultPrefabObjects) 에 `PF_Char_X` 가 있는지 확인. 없으면 `Tools > Fish-Networking > Utility > Refresh Default Prefabs`
- [ ] 스포너(이 범위 밖)가 선택된 Definition 의 `NetworkPrefab` 을 Instantiate → `ServerManager.Spawn(obj, 해당 연결)` 하는지 확인

**7. 테스트**
- [ ] 오프라인: 씬에 Variant 를 끌어다 놓고 T-포즈 없음, 소켓 위치, 콜라이더 크기 확인
- [ ] 호스트 1 + 클라 1 (빌드 1개 + 에디터, 또는 ParrelSync/Multiplayer Play Mode)
  - [ ] 카메라·HUD 가 *자기* 캐릭터에만 붙음, 남의 캐릭터는 입력을 받지 않음
  - [ ] 이동·점프 애니메이션이 양쪽 화면에서 재생
  - [ ] 스킬 3종: 쿨다운 표시, 상대 화면에서 모션·연출, 호스트에서 연출이 두 번 나오지 않음
  - [ ] 데미지 → 양쪽 체력바·이름표 갱신, 사망/부활
  - [ ] 늦게 들어온 클라: 이름·체력·사망 상태가 맞게 보임
  - [ ] 디스폰/재접속 시 HUD 가 비워지고 카메라가 새 캐릭터로 즉시 붙음

---

## 6. 멀티플레이 방식 비교와 추천

| | A. Variant 4개를 각각 네트워크 프리팹으로 등록 | B. 프리팹 1개 + characterId 동기화 후 런타임 외형 로드 |
|---|---|---|
| 스폰 | 서버가 선택된 Variant 를 Spawn — 끝 | 공통 프리팹 Spawn + SyncVar 설정 → 각 피어가 외형/능력 구성 |
| 스폰 직후 외형 | 프리팹에 들어 있어 즉시 정확 | SyncVar 도착 전까지 빈 몸 가능, 순서 처리 필요 |
| 캐릭터별 능력 | Variant 에 컴포넌트 추가 (목표 3 그대로) | 런타임 AddComponent/Instantiate. 능력이 NetworkBehaviour 면 **불가** (FishNet 은 서버·클라의 NetworkBehaviour 구성이 스폰 시점에 같아야 함) |
| 캐릭터별 크기·소켓·히트박스 | 에디터에서 Variant 마다 눈으로 조정 | 전부 Definition 데이터로 표현해야 함 |
| 매치 중 캐릭터 교체 | 디스폰 → 재스폰 | SyncVar 만 바꾸면 됨 |
| 네트워크 프리팹 수 | 캐릭터 수만큼 (4) | 1 |
| 공통 수정 전파 | Variant 상속으로 자동 | 자동 |
| 로딩 | 4명 모두 빌드에 포함 | Addressables 로 지연 로드 가능 |

**추천: A.** 캐릭터가 4명뿐이라 등록 비용이 없고, 캐릭터마다 몸 크기·소켓·히트박스를 에디터에서 맞추는 게 데이터로 표현하는 것보다 훨씬 싸다. "컴포넌트만 추가해서 확장"이 그대로 성립하고, 스폰 직후 외형이 정해지지 않는 상태가 없다.
B 로 바꿀 이유가 생기면(매치 중 교체, 캐릭터 수십 명, 스킨 시스템) 코드는 이미 대비돼 있다: 능력이 MonoBehaviour 이고, `CharacterVisualLoader.Build` 가 빈 Visual 슬롯도 처리하며, `CharacterCore.ApplyDefinition` / `CharacterNetworkSync.OnCharacterIdChanged` 자리가 있다.

### 특수 기능 발동 흐름이 표시된 위치

| 단계 | 어디서 실행 | 파일 · 메서드 |
|---|---|---|
| ① 입력 | 소유 클라 | `CharacterInput.Update` |
| ② 로컬 체크·예측·요청 | 소유 클라 | `AbilityController.TryActivate` → `CharacterAbility.CanActivateLocal`, `OnOwnerPredict` |
| ③ 서버 검증 (보안 경계) | 서버 | `AbilityController.ServerRequestActivate` [ServerRpc] → `CharacterAbility.ServerCanActivate` |
| ③' 거절 통보 | 서버 → 소유 클라 | `AbilityController.TargetActivationRejected` [TargetRpc] → `OnActivationRejected` |
| ④ 결과 적용 | 서버 | `CharacterAbility.ServerActivate` (데미지 `CharacterHealth.ServerApplyDamage`, 스폰 등) |
| ⑤ 연출 브로드캐스트 | 모든 클라 | `AbilityController.ObserversActivated` [ObserversRpc] → `Bridge.PlaySkill`, `OnObserversActivated` |
| ⑥ 지속 상태 동기화 | 서버 → 모든 클라 (늦은 접속 포함) | `CharacterHealth` SyncVar, 투사체 NetworkObject, NetworkTransform/예측 |

원칙: **Server* 메서드에는 연출을 넣지 않는다.** 호스트에서는 서버 코드와 클라 코드가 한 프로세스에서 모두 돌아 연출이 두 번 나온다.

---

## 7. [TODO: 결정 필요] 모음

| 항목 | 위치 | 선택지 |
|---|---|---|
| 이동 권한 모델 | CharacterMovement | 클라 권한 + NetworkTransform (협동) / 서버 권한 예측 Replicate·Reconcile (대전) |
| 원격 애니메이션 동기화 | CharacterAnimatorBridge | NetworkAnimator / 각 클라가 직접 계산 |
| 전용 서버의 Animator | CharacterVisualLoader.SetupAnimator | 끄기 (본·애니 이벤트 판정 안 씀) / AlwaysAnimate |
| 쿨다운 시계 | CharacterAbility.CooldownRemaining | 로컬 시간 / FishNet 틱 |
| 예측 연출 중복 방지 | AbilityController.ObserversActivated | 능력 플래그 / ExcludeOwner |
| 투사체 방식 | ProjectileAbility | 네트워크 투사체 / 연출 투사체 + 서버 판정 |
| 부위 판정 | Hurtbox | 단일 캡슐 / 본 Hurtbox (서버 AlwaysAnimate 필요) |
| 지연 보상 | Hitbox | 없음 / FishNet Lag Compensation |
| 팀 개념 | CharacterNetworkSync | 없음(협동) / 팀 번호 |
| characterId 형식 | CharacterDefinition | int / GUID 문자열 |
| 카메라 방식 | PlayerCameraRig | 3인칭 확정. 현재 OrbitalFollow(자유 궤도) — 숄더뷰가 필요하면 ThirdPersonFollow + CameraTarget 회전 코드 |
| 스킬 키 배치 | CharacterInput | InputSystem_Actions 에 Skill1/Skill2/Ultimate 추가 |
| Locomotion 형태 | AC_Character_Base | 2D 스트레이프 / 1D |
