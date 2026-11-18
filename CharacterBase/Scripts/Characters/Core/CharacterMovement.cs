using FishNet.Object;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 공통 이동 모터 (CharacterController 기반): 걷기/달리기/회전/점프/중력/외부 힘(대시·넉백)/루트모션 합성.
    /// 입력은 소유 클라의 CharacterInput 이 SetInput 으로 넣는다. 실제 이동 계산은 "이동 권한을 가진 쪽"에서만 한다.
    ///
    /// [TODO: 결정 필요] 이동 권한 모델
    ///   (1) 클라 권한: 소유 클라가 Simulate → Root 의 NetworkTransform(Client Authoritative) 이 위치를 동기화.
    ///       장점: 구현 최소, 입력 지연 0. 단점: 위치 치트에 무방비 → 서버는 속도 상한 검사 정도만.
    ///   (2) 서버 권한 + 예측: FishNet Prediction 의 [Replicate]/[Reconcile].
    ///       MoveInput 을 IReplicateData 로, 위치·속도를 IReconcileData 로 만들고 OnTick 에서 Simulate.
    ///       장점: 치트 방지·공정한 판정. 단점: 구현량 큼. CharacterController 는 재시뮬레이션 시
    ///       enabled 토글 / Physics.SyncTransforms 처리가 필요.
    ///   협동(PvE) 4인이면 (1)로 시작, 대전(PvP)이면 (2) 권장.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMovement : NetworkBehaviour
    {
        [SerializeField] private CharacterController controller;
        [Tooltip("Root/GroundCheck. 캡슐 바닥보다 살짝 아래.")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.25f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float gravity = -20f;

        private CharacterCore _core;
        private CharacterStats _stats;
        private MoveInput _input;
        private Vector3 _velocity;       // 수직 속도 + 외부 힘(대시·넉백) 합
        private int _lockCount;          // > 0 이면 입력 이동 금지 (스킬 시전, 사망, 스턴)
        private bool _useRootMotion;

        public bool IsGrounded { get; private set; }
        public Vector3 Velocity => _velocity;
        public bool IsLocked => _lockCount > 0;

        // 호출: CharacterCore.OnStartNetwork
        // 구현: _core = core; _stats = core.Definition.BaseStats;
        //       core.AnimatorBridge.RootMotion += HandleRootMotion; (해제는 OnStopNetwork 에서)
        //       (1) 클라 권한이면: 비소유 클라에서는 controller.enabled 유지(충돌용)하되 Simulate 하지 않음
        //       (2) 예측이면: TimeManager.OnTick += OnTick 구독
        public void Initialize(CharacterCore core) { }

        /// <summary>이번 프레임/틱 입력. 소유 클라의 CharacterInput 만 호출한다.</summary>
        // 구현: _input = input; (점프는 "눌린 순간" 이므로 Simulate 에서 소비 후 false 로)
        public void SetInput(in MoveInput input) { }

        // 호출: Unity 매 프레임.
        // 구현:
        //   (1) 클라 권한: if (!IsOwner) { 원격 캐릭터 애니 갱신만 — NetworkTransform 이 옮긴 위치 변화량으로 속도 추정 후
        //                    _core.AnimatorBridge.SetLocomotion(...) (NetworkAnimator 를 쓰면 생략) ; return; }
        //                  Simulate(_input, Time.deltaTime);
        //   (2) 예측: 비워 두고 OnTick 사용 (FishNet 틱과 Update 프레임이 다르므로 섞지 말 것)
        private void Update() { }

        // 핵심 이동 1스텝. 결정적이어야 한다 (예측 재시뮬레이션 대비: Time.* 대신 인자 delta 만 사용).
        // 구현 순서:
        //   1) IsGrounded = CheckGround(); 착지 중이고 _velocity.y < 0 이면 작은 음수(-2)로 고정 (경사로 들뜸 방지)
        //   2) 잠금이 아니면: 목표 수평속도 = input.worldDirection * moveSpeed * (sprint ? 배율 : 1)
        //      이동 방향으로 turnSpeed 만큼 회전 (Quaternion.RotateTowards) — [TODO: 결정 필요] 조준 중엔 카메라 방향 고정 회전?
        //   3) 점프: IsGrounded && input.jump → _velocity.y = sqrt(2 * jumpHeight * -gravity); _core.AnimatorBridge.TriggerJump()
        //   4) _velocity.y += gravity * delta; 외부 힘 수평 성분은 감쇠
        //   5) controller.Move((수평 + _velocity) * delta)  — Move 는 프레임당 1회만 (여러 번 부르면 충돌 처리가 어긋남)
        //   6) _core.AnimatorBridge.SetLocomotion(정규화 속도, 로컬 이동방향, IsGrounded, _velocity.y)
        private void Simulate(in MoveInput input, float delta) { }

        // CharacterController.isGrounded 는 Move 직후에만 유효하고 경사·계단에서 깜빡이므로 별도 검사.
        // 구현: Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore)
        //       (Hurtbox 등 자기 트리거가 걸리지 않게 groundMask 에서 Character/Hurtbox 레이어 제외)
        private bool CheckGround() { return default; }

        /// <summary>외부 속도 추가 (대시, 넉백, 점프패드).</summary>
        // 호출: 능력(DashAbility), 피격 넉백. 이동 권한을 가진 쪽에서 호출해야 반영된다 ((1)이면 소유 클라, (2)면 Replicate 안).
        // 구현: _velocity += velocity;
        public void AddImpulse(Vector3 velocity) { }

        /// <summary>입력 이동 잠금. 중첩 가능 (스킬 + 스턴 동시).</summary>
        // 구현: _lockCount++ / Mathf.Max(0, _lockCount - 1). 사망 시 Health 가 Add, 부활 시 Remove.
        public void AddMoveLock() { }
        public void RemoveMoveLock() { }

        /// <summary>특정 스킬 동안만 애니메이션 루트모션으로 이동.</summary>
        // 호출: 루트모션을 쓰는 능력의 시작/끝. 구현: _useRootMotion = enabled;
        public void SetRootMotion(bool enabled) { }

        // 호출: CharacterAnimatorBridge.RootMotion 이벤트 (모델 Animator → AnimatorEventRelay → Bridge → 여기)
        // 구현: if (!_useRootMotion) return; controller.Move(deltaPosition); transform.rotation *= deltaRotation;
        //   주의: 루트모션은 애니메이션이 도는 쪽에서만 생긴다. 전용 서버에서 Animator 를 끄면 서버 위치가 어긋나므로
        //         네트워크 게임플레이용 이동은 코드(AddImpulse)로, 루트모션은 연출용(작은 스텝)으로만 권장.
        private void HandleRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { }

        /// <summary>순간이동 (리스폰, 텔레포트 스킬). 서버에서 호출.</summary>
        // 구현: controller.enabled = false → transform.SetPositionAndRotation → controller.enabled = true
        //       (CharacterController 가 켜져 있으면 다음 Move 가 위치를 되돌린다)
        //       (1) 클라 권한이면 소유 클라에 TargetRpc 로 알려 그쪽에서 이동시켜야 함 / NetworkTransform 의 Teleport 옵션 확인
        //       (2) 예측이면 Reconcile 데이터에 반영
        public void Teleport(Vector3 position, Quaternion rotation) { }

        // 에디터 시각화. 구현: Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius)
        private void OnDrawGizmosSelected() { }
    }

    /// <summary>
    /// 한 프레임(틱)의 이동 입력. 서버는 카메라를 모르므로 소유 클라가 카메라 기준 → 월드 방향으로 변환해서 넣는다.
    /// (2) 예측 방식으로 가면 이 struct 가 IReplicateData 를 구현하도록 바꾼다.
    /// </summary>
    public struct MoveInput
    {
        public Vector3 worldDirection; // 월드 XZ, 길이 0~1
        public bool jump;              // 이번 프레임에 눌림
        public bool sprint;            // 유지 중
    }
}
