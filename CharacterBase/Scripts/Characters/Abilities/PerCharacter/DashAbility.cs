using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// [캐릭터A] 예시 — 대시. 조준(또는 이동) 방향으로 짧게 돌진.
    /// "누가 실제로 캐릭터를 옮기는가"는 이동 권한 모델(CharacterMovement 의 TODO)에 따라 다르다:
    ///   클라 권한 이동 : 소유 클라가 OnOwnerPredict 에서 Movement.AddImpulse → NetworkTransform 이 동기화. 서버는 쿨다운만 검증.
    ///   서버 권한 예측 : 대시 요청을 Replicate 입력에 실어 양쪽이 같은 시뮬레이션을 돌린다.
    /// </summary>
    public sealed class DashAbility : CharacterAbility
    {
        [SerializeField] private float distance = 5f;
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private bool invulnerableWhileDashing;

        // 구현: base.Initialize(core); 이후 필요한 참조 캐시
        public override void Initialize(CharacterCore core) { base.Initialize(core); }

        // 구현: base 조건 + 이동 잠금 아님 (+ 공중 대시 허용 여부 [TODO: 결정 필요])
        public override bool CanActivateLocal(in AbilityContext ctx) { return default; }

        // 구현 (클라 권한 이동일 때): 방향 = ctx.direction 의 XZ 정규화, 속도 = distance / duration 으로 Core.Movement.AddImpulse
        //       duration 동안 Core.Movement.AddMoveLock → 끝나면 RemoveMoveLock
        public override void OnOwnerPredict(in AbilityContext ctx) { }

        // 구현: base 조건 + (클라 권한이면) 직전 위치 대비 이동 거리 상한 검사 정도
        public override bool ServerCanActivate(in AbilityContext ctx) { return default; }

        // 구현: 쿨다운은 컨트롤러가 시작. invulnerableWhileDashing 이면 duration 동안 무적 → 이 클래스가 IDamageModifier 를 구현해 처리
        public override void ServerActivate(in AbilityContext ctx) { }

        // 구현: 잔상/트레일 VFX, 사운드 (소유자가 예측으로 이미 틀었으면 생략)
        public override void OnObserversActivated(in AbilityContext ctx) { }

        // 구현: 클라 권한 이동이면 이미 움직인 위치를 되돌리기 어렵다 → 서버는 쿨다운 불일치 정도면 거절하지 말고 맞춰 주는 편이 낫다
        public override void OnActivationRejected() { }
    }
}
