using System;
using FishNet.Component.Animating;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 로직 ↔ Animator 사이의 유일한 통로.
    ///  - 로직(Movement/Health/Ability)은 Animator 를 직접 만지지 않고 이 브리지의 메서드만 부른다
    ///    → 모델이 바뀌어도(Animator 인스턴스 교체) 로직 쪽 참조가 깨지지 않는다.
    ///  - 파라미터 이름은 AnimParams 상수로만 쓴다 (문자열 오타 = 조용한 버그).
    ///
    /// 모델 → 루트 전달 경로 (Relay 가 필요한 이유):
    ///   Unity 는 OnAnimatorMove 와 애니메이션 이벤트 함수를 "Animator 가 붙은 GameObject" 의 컴포넌트에서만 찾는다.
    ///   Animator 는 Root/Visual/Model 에 있고 로직은 Root 에 있으므로, 모델 루트에 얇은 AnimatorEventRelay 를 붙여 위로 넘긴다.
    ///     [Model] Animator ─ OnAnimatorMove() ─▶ AnimatorEventRelay ─▶ Bridge.ReceiveRootMotion ─▶ RootMotion 이벤트 ─▶ CharacterMovement
    ///     [Clip]  Animation Event "AnimEvent"(string) ─▶ AnimatorEventRelay.AnimEvent ─▶ Bridge.ReceiveAnimationEvent ─▶ AnimationEventRaised ─▶ 능력/사운드
    /// </summary>
    public sealed class CharacterAnimatorBridge : MonoBehaviour
    {
        // [TODO: 결정 필요] 원격 캐릭터 애니메이션 동기화 방식
        //   (a) FishNet NetworkAnimator 를 Root 에 두고 여기 연결 → 소유자(또는 서버)의 Float/Bool 이 자동 동기화. 구현 쉬움.
        //   (b) 비워 두고 각 클라가 동기화된 위치 변화/SyncVar 로 파라미터를 직접 계산 → 대역폭 절약, 구현 조금 더.
        //   어느 쪽이든 스킬·피격 트리거는 RPC 흐름(AbilityController/Health)에서 각 클라가 로컬로 건다.
        [SerializeField] private NetworkAnimator networkAnimator;

        private Animator _animator;

        public Animator Animator => _animator;

        /// <summary>(deltaPosition, deltaRotation). CharacterMovement 가 구독.</summary>
        public event Action<Vector3, Quaternion> RootMotion;
        /// <summary>AnimEvents.* 문자열. 능력/사운드/이펙트가 구독. 서버 판정 타이밍에는 쓰지 말 것 (연출 전용).</summary>
        public event Action<string> AnimationEventRaised;

        // 호출: CharacterCore.Awake (VisualLoader.Build 직후), 접근 B 에선 VisualLoader.VisualChanged
        // 구현:
        //   _animator = animator;
        //   모델 루트에 AnimatorEventRelay 가 없으면 AddComponent → relay.Bind(this)
        //   if (networkAnimator != null) networkAnimator.SetAnimator(animator);
        //   개발 빌드에서만: AnimParams 의 모든 이름이 animator.parameters 에 있는지 검사 (오버라이드/베이스 불일치 조기 발견)
        public void Bind(Animator animator) { }

        // 호출: CharacterMovement.Simulate (이동을 계산한 쪽), 원격 캐릭터는 TODO (a)/(b) 에 따라
        // 구현: SetFloat(AnimParams.MoveSpeed, speed01, 0.1f, Time.deltaTime) — dampTime 으로 떨림 완화
        //       MoveX/MoveY = localMove, IsGrounded, VerticalSpeed
        public void SetLocomotion(float speed01, Vector2 localMove, bool grounded, float verticalSpeed) { }

        // 구현: _animator.SetTrigger(AnimParams.Jump)
        public void TriggerJump() { }

        // 호출: AbilityController.ObserversActivated (흐름⑤, 각 클라)
        // 구현: SetInteger(AnimParams.SkillIndex, (int)slot + 1); SetTrigger(AnimParams.Skill);
        //   networkAnimator.SetTrigger 가 아니라 _animator.SetTrigger — 각 클라가 RPC 를 받아 직접 거므로, 네트워크로 또 보내면 이중 재생.
        public void PlaySkill(AbilitySlot slot) { }

        // 구현: SetBool(AnimParams.IsChanneling, channeling) — SkillN_Loop 유지
        public void SetChanneling(bool channeling) { }

        // 호출: CharacterHealth.ObserversDamaged. 구현: SetTrigger(AnimParams.Hit)
        public void PlayHit() { }

        // 호출: CharacterHealth.OnDeadChanged. 구현: SetBool(AnimParams.IsDead, dead)
        public void SetDead(bool dead) { }

        // 구현: SetFloat(AnimParams.ActionSpeed, multiplier) — 공격속도 버프 등. 스킬 상태의 Speed Multiplier 파라미터로 연결돼 있어야 함.
        public void SetActionSpeed(float multiplier) { }

        // 호출: AnimatorEventRelay.OnAnimatorMove. 구현: RootMotion?.Invoke(deltaPosition, deltaRotation)
        internal void ReceiveRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { }

        // 호출: AnimatorEventRelay.AnimEvent. 구현: AnimationEventRaised?.Invoke(eventName)
        internal void ReceiveAnimationEvent(string eventName) { }
    }

    /// <summary>공통 Animator 파라미터. AC_Character_Base 와 이름이 1:1 로 같아야 한다 (문서의 파라미터 표 참고).</summary>
    public static class AnimParams
    {
        public static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public static readonly int MoveX = Animator.StringToHash("MoveX");
        public static readonly int MoveY = Animator.StringToHash("MoveY");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Skill = Animator.StringToHash("Skill");
        public static readonly int SkillIndex = Animator.StringToHash("SkillIndex");
        public static readonly int IsChanneling = Animator.StringToHash("IsChanneling");
        public static readonly int Hit = Animator.StringToHash("Hit");
        public static readonly int IsDead = Animator.StringToHash("IsDead");
        public static readonly int ActionSpeed = Animator.StringToHash("ActionSpeed");
    }

    /// <summary>클립 Animation Event 의 String 파라미터 값. 클립 쪽 Function 은 항상 "AnimEvent".</summary>
    public static class AnimEvents
    {
        public const string Footstep = "Footstep";
        public const string Release = "Release";       // 스킬 발사/타격 순간 (연출 동기용)
        public const string HitOpen = "HitOpen";       // 근접 판정 시작 (클라 연출용. 서버 판정은 능력 데이터의 시간으로)
        public const string HitClose = "HitClose";
        public const string SkillEnd = "SkillEnd";
    }
}
