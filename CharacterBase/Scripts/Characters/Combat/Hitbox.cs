using System.Collections.Generic;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 근접 공격 판정 볼륨. 서버에서만 판정한다. 트리거 이벤트 대신 물리 쿼리(OverlapBox)를 쓴다:
    ///  - 트리거 이벤트는 Rigidbody 조합·물리 프레임에 의존해 "언제 켜고 끌지" 를 서버 코드가 정확히 통제하기 어렵다.
    ///  - 열고 닫는 시점은 애니메이션 이벤트가 아니라 능력 데이터의 시간(초)으로 (전용 서버는 애니메이션을 안 돌릴 수 있음).
    /// 무기 길이가 캐릭터마다 다르므로 Variant 에서 크기/위치를 조정한다. Root/Combat/Hitboxes 아래 (루트 기준, 본에 붙이지 않음).
    /// </summary>
    public sealed class Hitbox : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private Vector3 halfExtents = new Vector3(0.6f, 0.6f, 0.8f);
        [SerializeField] private LayerMask hurtboxMask;
        [SerializeField] private int maxHitsPerQuery = 8;

        private readonly HashSet<CharacterHealth> _alreadyHit = new HashSet<CharacterHealth>();
        private Collider[] _buffer;
        private DamageInfo _template;
        private bool _open;

        /// <summary>판정 시작. 서버 전용.</summary>
        // 호출: 근접 능력의 ServerActivate (선딜 후), 또는 기본 공격 로직
        // 구현: _template = template; _alreadyHit.Clear(); _buffer ??= new Collider[maxHitsPerQuery]; _open = true;
        public void ServerOpen(DamageInfo template) { }

        // 구현: _open = false;
        public void ServerClose() { }

        // 구현: if (!_open) return;
        //   Physics.OverlapBoxNonAlloc(transform.position, halfExtents, _buffer, transform.rotation, hurtboxMask, QueryTriggerInteraction.Collide)
        //   → 각 콜라이더의 Hurtbox: owner 자신 제외, 팀 체크([TODO] NetworkSync.TeamId), _alreadyHit 중복 제외
        //   → hurtbox.Core.Health.ServerApplyDamage(_template 에 DamageMultiplier·hitPoint 반영)
        //   [TODO: 결정 필요] 지연 보상: 공격자 화면 기준 판정이 필요하면 FishNet Lag Compensation(ColliderRollback) 검토
        //   [TODO: 결정 필요] FixedUpdate 대신 FishNet TimeManager.OnTick 에서 돌릴지 (예측 이동을 쓰면 틱 기준이 맞다)
        private void FixedUpdate() { }

        // 구현: Gizmos.matrix = transform.localToWorldMatrix; Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        private void OnDrawGizmosSelected() { }
    }
}
