using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// [캐릭터B] 예시 — 실드. 일정 시간 받는 데미지를 줄이거나 일정량 흡수.
    /// IDamageModifier 를 구현했으므로 CharacterHealth 를 고치지 않아도 데미지 계산에 끼어든다 (확장 지점 예시).
    /// </summary>
    public sealed class ShieldAbility : CharacterAbility, IDamageModifier
    {
        [SerializeField] private float duration = 4f;
        [Range(0f, 1f)]
        [SerializeField] private float damageReduction = 0.5f;
        [SerializeField] private float maxAbsorb = 100f;
        [SerializeField] private GameObject shieldVfx;   // Abilities/Ability_Shield 자식에 둔 이펙트 (기본 비활성)

        // 구현: 실드 남은 시간 > 0
        public override bool IsActive => default;

        // 구현: 서버 상태 세팅 — 남은 시간 = duration, 남은 흡수량 = maxAbsorb
        public override void ServerActivate(in AbilityContext ctx) { }

        // 구현: shieldVfx.SetActive(true); Core.AnimatorBridge.SetChanneling(true) (유지 모션이 있으면)
        //   [TODO: 결정 필요] 늦게 접속한 클라에게도 실드가 보여야 하면 SyncVar<bool> 가진 NetworkBehaviour 를 같은 오브젝트에 추가
        public override void OnObserversActivated(in AbilityContext ctx) { }

        // 구현: 서버 상태 해제 (남은 시간 = 0)
        public override void ServerCancel() { }

        // 구현: shieldVfx.SetActive(false); SetChanneling(false)
        public override void OnObserversCancelled() { }

        // 구현: 남은 시간 감소. 서버에서 0 이 되면 Core.Abilities.ServerCancel(Slot) → ServerCancel + 모든 클라 OnObserversCancelled
        public override void Tick(float deltaTime) { }

        // CharacterHealth.ServerApplyDamage 가 서버에서 호출.
        // 구현: if (!IsActive) return; 줄일 양 = info.amount * damageReduction 을 남은 흡수량 한도 안에서 → info.amount 감소, 흡수량 차감
        public void ModifyIncomingDamage(ref DamageInfo info) { }
    }
}
