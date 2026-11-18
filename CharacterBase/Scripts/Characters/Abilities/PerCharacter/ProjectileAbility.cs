using FishNet.Object;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// [캐릭터C] 예시 — 투사체 발사.
    /// [TODO: 결정 필요] 투사체 방식
    ///   (a) 네트워크 투사체: 서버가 NetworkObject 프리팹을 Spawn, 위치는 NetworkTransform/예측. 간단하지만 투사체가 많으면 대역폭 부담.
    ///   (b) 연출 투사체: ⑤에서 각 클라가 같은 origin/direction 으로 로컬(비네트워크) 투사체를 만들고, 판정은 서버가 계산(레이/스윕). 빠른 탄에 유리.
    /// </summary>
    public sealed class ProjectileAbility : CharacterAbility
    {
        [SerializeField] private NetworkObject projectilePrefab;   // (a) 용. DefaultPrefabObjects 에 등록돼야 스폰 가능
        [SerializeField] private GameObject cosmeticProjectilePrefab; // (b) 용
        [SerializeField] private float speed = 25f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private float maxRange = 40f;

        // 구현: base 조건 + ctx.direction 정규화 확인 + ctx.origin 이 Core.Visual.GetSocket(SocketId.ProjectileOrigin) 와 허용 오차 내
        public override bool ServerCanActivate(in AbilityContext ctx) { return default; }

        // 구현: 발사 원점은 서버 자기 루트 소켓 (클라가 보낸 origin 대신) — 본 소켓은 서버에서 믿을 수 없음
        //   (a) var go = Instantiate(projectilePrefab, origin, rot); 초기화(속도·데미지·발사자 ObjectId); Core.ServerManager.Spawn(go.gameObject, Core.Owner);
        //   (b) 서버에서 Physics.SphereCast 로 즉시/틱 단위 판정 → Hurtbox → Health.ServerApplyDamage
        public override void ServerActivate(in AbilityContext ctx) { }

        // 구현: 발사 모션은 컨트롤러가 PlaySkill 로 처리. 머즐 플래시 VFX (본 소켓 RightHand 등 — 연출이므로 본 소켓 OK)
        //       (b) 방식이면 여기서 cosmeticProjectilePrefab 생성
        public override void OnObserversActivated(in AbilityContext ctx) { }
    }
}
