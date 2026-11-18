using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters
{
    /// <summary>
    /// 소유 클라이언트 전용 입력 읽기 (Input System).
    /// 베이스 프리팹에서는 컴포넌트를 꺼 두고(enabled = false) LocalPlayerBinder 가 "내 캐릭터"일 때만 켠다.
    /// → 남의 캐릭터가 내 키보드 입력을 받는 사고를 구조적으로 막는다.
    /// InputActionReference 는 에셋 단위로 공유되므로, 켜진 인스턴스가 하나뿐이어야 Enable/Disable 이 서로 꼬이지 않는다.
    /// </summary>
    public sealed class CharacterInput : MonoBehaviour
    {
        [SerializeField] private CharacterCore core;

        [Header("Input System 액션 (InputSystem_Actions 의 Player 맵)")]
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private InputActionReference sprint;
        // [TODO: 결정 필요] 기본 InputSystem_Actions 에는 스킬 액션이 없다 → Skill1/Skill2/Ultimate 액션 추가 (키 배치 결정)
        [SerializeField] private InputActionReference skill1;
        [SerializeField] private InputActionReference skill2;
        [SerializeField] private InputActionReference ultimate;

        [Header("조준")]
        [SerializeField] private LayerMask aimMask = ~0;
        [SerializeField] private float aimMaxDistance = 100f;

        // 구현: 각 액션 .action.Enable()
        private void OnEnable() { }

        // 구현: 각 액션 .action.Disable(); core.Movement.SetInput(default) — 입력이 끊기면 제자리에 멈추게
        private void OnDisable() { }

        // [흐름①] 소유 클라: 입력 감지
        // 구현:
        //   core.Movement.SetInput(ReadMoveInput());
        //   if (skill1.action.WasPressedThisFrame()) core.Abilities.TryActivate(AbilitySlot.Skill1, BuildAimContext());
        //   skill2 / ultimate 동일. (채널링 해제가 필요하면 WasReleasedThisFrame → core.Abilities.TryCancel)
        private void Update() { }

        // 구현: move 값(Vector2)을 Camera.main 의 forward/right 를 XZ 평면에 투영한 축으로 변환 → worldDirection (길이 ≤ 1)
        //       jump = jump.action.WasPressedThisFrame(), sprint = sprint.action.IsPressed()
        private MoveInput ReadMoveInput() { return default; }

        // 구현: Camera.main 화면 중앙 레이 → Physics.Raycast(aimMaxDistance, aimMask) → targetPoint
        //       origin = core.Visual.GetSocket(SocketId.ProjectileOrigin).position, direction = (targetPoint - origin).normalized
        //       targetObjectId = 맞은 Hurtbox 의 core.ObjectId, 없으면 -1
        private AbilityContext BuildAimContext() { return default; }
    }
}
