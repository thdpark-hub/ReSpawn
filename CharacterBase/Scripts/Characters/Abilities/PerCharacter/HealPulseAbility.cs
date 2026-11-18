using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// [캐릭터D] 예시 — 범위 회복 펄스. 주변 아군 체력을 회복.
    /// 범위 중심은 루트 소켓(Center/Feet)을 쓴다 — 서버 판정이므로 본 소켓 금지.
    /// </summary>
    public sealed class HealPulseAbility : CharacterAbility
    {
        [SerializeField] private float radius = 6f;
        [SerializeField] private float healAmount = 30f;
        [SerializeField] private LayerMask hurtboxMask;
        [SerializeField] private bool includeSelf = true;

        // 구현: base 조건 (대상이 없어도 발동 허용할지 [TODO: 결정 필요])
        public override bool ServerCanActivate(in AbilityContext ctx) { return default; }

        // 구현: Physics.OverlapSphereNonAlloc(Core.Visual.GetSocket(SocketId.Feet).position, radius, buffer, hurtboxMask, QueryTriggerInteraction.Collide)
        //   → Hurtbox.Core 중복 제거 → 같은 팀(NetworkSync.TeamId)이고 살아 있으면 Health.ServerHeal(healAmount)
        //   includeSelf 가 false 면 자기 Core 제외
        public override void ServerActivate(in AbilityContext ctx) { }

        // 구현: 바닥 원형 VFX (반경 = radius), 사운드. 회복 수치 팝업은 각 대상의 HealthChanged 쪽에서
        public override void OnObserversActivated(in AbilityContext ctx) { }
    }
}
