using System;
using FishNet.Object;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 루트 허브. 같은 프리팹의 공통 컴포넌트 참조를 들고, 초기화 순서를 이 파일 한 곳에서 관리한다.
    ///
    /// 규칙
    ///  - 다른 캐릭터 컴포넌트는 Awake/Start 에서 서로를 찾거나 초기화하지 않는다. Core 가 순서대로 Initialize(core) 를 부른다.
    ///    → 초기화 순서 버그가 이 파일에서만 생기게 하기 위함.
    ///  - 컴포넌트끼리는 core.Health, core.Movement 처럼 Core 를 통해 접근한다 (GetComponent 남발 금지).
    ///  - 참조는 베이스 프리팹에서 인스펙터로 연결 (OnValidate 가 비어 있는 칸을 자동 채움).
    ///
    /// FishNet 콜백 순서 (같은 NetworkObject 안의 모든 NetworkBehaviour 에 대해 단계별로 전부 호출된 뒤 다음 단계):
    ///   OnStartNetwork → OnStartServer → OnOwnershipServer   (서버 쪽)
    ///   OnStartNetwork → OnStartClient → OnOwnershipClient   (클라 쪽, 호스트는 OnStartNetwork 를 한 번만)
    ///   그래서 여기 OnStartNetwork 에서 초기화를 끝내면 LocalPlayerBinder.OnOwnershipClient 시점엔 준비가 끝나 있다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class CharacterCore : NetworkBehaviour
    {
        [Header("데이터 (Variant 에서 교체)")]
        [SerializeField] private CharacterDefinition definition;

        [Header("공통 컴포넌트 (베이스 프리팹 Root)")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private CharacterMovement movement;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private CharacterNetworkSync networkSync;
        [SerializeField] private CharacterVisualLoader visual;
        [SerializeField] private CharacterAnimatorBridge animatorBridge;
        [SerializeField] private AbilityController abilities;
        [SerializeField] private CharacterInput input;

        [Header("슬롯 (베이스 프리팹 자식)")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform nameplateAnchor;

        public CharacterDefinition Definition => definition;
        public CharacterController Controller => controller;
        public CharacterMovement Movement => movement;
        public CharacterHealth Health => health;
        public CharacterNetworkSync NetworkSync => networkSync;
        public CharacterVisualLoader Visual => visual;
        public CharacterAnimatorBridge AnimatorBridge => animatorBridge;
        public AbilityController Abilities => abilities;
        public CharacterInput Input => input;
        public Transform CameraTarget => cameraTarget;
        public Transform NameplateAnchor => nameplateAnchor;

        /// <summary>OnStartNetwork 의 초기화가 끝나면 true.</summary>
        public bool IsReady { get; private set; }

        /// <summary>초기화 완료 시 1회. 늦게 붙는 외부 시스템(관전 UI 등)이 구독.</summary>
        public event Action Ready;

        // 호출: Unity, Instantiate 직후 (네트워크 시작 전). 오프라인 테스트(씬에 직접 배치)에서도 불린다.
        // 구현: 네트워크와 무관한 로컬 준비만.
        //   1) 필수 참조 null 검사 → 없으면 Debug.LogError(this) 후 enabled = false
        //   2) visual.Build(definition)  — 접근 A 는 Definition 이 프리팹에 박혀 있으므로 여기서 외형을 만들 수 있다
        //   3) animatorBridge.Bind(visual.Animator)
        //   (접근 B 라면 2·3 은 CharacterNetworkSync 의 characterId 수신 후 ApplyDefinition 에서)
        private void Awake() { }

        // 호출: FishNet. 서버·클라 모두. OnStartServer/OnStartClient 보다 먼저.
        // 구현: 순서가 의존성이다. 바꿀 땐 이유를 주석으로.
        //   1) movement.Initialize(this)      — 스탯(이동속도·점프) 적용
        //   2) health.Initialize(this)        — 서버면 최대/현재 체력 세팅 (내부에서 IsServerInitialized 분기)
        //   3) networkSync.Initialize(this)   — SyncVar 구독, (서버) characterId 기록
        //   4) abilities.Initialize(this)     — 능력 수집·슬롯 매핑 (Health 이벤트 구독하므로 2 이후)
        //   5) IsReady = true; Ready?.Invoke();
        // 주의: 여기서는 IsOwner 를 쓰지 말 것 (FishNet 이 경고). 소유자 전용 처리는 LocalPlayerBinder.OnOwnershipClient.
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
        }

        // 호출: FishNet, 디스폰/파괴 직전. 서버·클라 모두.
        // 구현: IsReady = false; 능력 취소/정리; Initialize 에서 구독한 이벤트 해제 (각 컴포넌트에 Shutdown 메서드를 두고 역순 호출).
        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
        }

        /// <summary>런타임에 캐릭터 정의를 교체한다. 접근 B(단일 프리팹 + characterId) 전용. 접근 A 에서는 호출하지 않는다.</summary>
        // 호출: CharacterNetworkSync.OnCharacterIdChanged (서버·클라 각자)
        // 구현: definition = newDefinition → visual.Build → animatorBridge.Bind → (서버) 스탯 재적용 → abilities 재구성
        public void ApplyDefinition(CharacterDefinition newDefinition) { }

        // 에디터 전용. 컴포넌트 추가/값 변경 시 호출 (NetworkBehaviour 가 이미 정의하므로 override).
        // 구현: 비어 있는 참조를 GetComponent / transform.Find("CameraTarget") 등으로 채운다.
        protected override void OnValidate()
        {
            base.OnValidate();
        }
    }
}
