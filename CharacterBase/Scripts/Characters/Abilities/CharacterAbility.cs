using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 캐릭터별 특수 기능의 베이스. Variant 의 Root/Abilities 아래 자식 오브젝트에 하나씩 붙이고 Slot 을 지정한다.
    /// 공통 코드(AbilityController 등)를 고치지 않고, 이 클래스를 상속한 컴포넌트를 추가하는 것만으로 확장된다.
    ///
    /// 네트워크: MonoBehaviour 다 (NetworkBehaviour 아님).
    ///   RPC·검증 흐름은 AbilityController 한 곳에 모으고, 능력은 "어느 쪽에서 무엇을 할지"만 오버라이드한다.
    ///   → 능력마다 RPC 를 새로 짜다 서버 검증을 빠뜨리는 실수를 막고, 런타임에 능력을 붙여도(접근 B) 동작한다.
    ///   능력 전용 지속 상태를 늦게 접속한 클라에게도 보여야 하면(예: 실드 켜짐) 같은 오브젝트에 작은 NetworkBehaviour + SyncVar 를
    ///   함께 둔다 (접근 A 에서만 가능). [TODO: 결정 필요]
    ///
    /// 메서드별 실행 위치 (흐름 번호는 AbilityController 주석 참고):
    ///   Initialize                          서버·모든 클라      Core 초기화 시
    ///   CanActivateLocal                    소유 클라           흐름② 서버 왕복 전 거르기 (UX 용, 보안 아님)
    ///   OnOwnerPredict                      소유 클라           흐름② 즉시 반응 연출/이동
    ///   ServerCanActivate                   서버                흐름③ 권위 있는 검증 (보안 경계)
    ///   ServerActivate                      서버                흐름④ 게임플레이 결과 (데미지·스폰·버프)
    ///   OnObserversActivated                모든 클라           흐름⑤ 연출 (애니·VFX·SFX)
    ///   OnActivationRejected                소유 클라           흐름③' 예측 되돌리기
    ///   ServerCancel / OnObserversCancelled 서버 / 모든 클라    취소
    ///   Tick                                서버·모든 클라      매 프레임 (채널링·지속형)
    ///   원칙: Server* 에는 연출을 넣지 않는다 — 호스트에선 서버 코드와 클라 코드가 한 프로세스에서 둘 다 돌아 연출이 두 번 나온다.
    /// </summary>
    public abstract class CharacterAbility : MonoBehaviour
    {
        [SerializeField] private AbilitySlot slot;

        public AbilitySlot Slot => slot;
        /// <summary>Definition 에서 읽은 표시·튜닝 데이터 (아이콘, 쿨다운).</summary>
        public AbilityInfo Info { get; private set; }
        protected CharacterCore Core { get; private set; }

        public Sprite Icon => Info.icon;
        public float CooldownDuration => Info.cooldown;

        /// <summary>남은 쿨다운(초). PlayerHUD 가 매 프레임 읽는다.</summary>
        // 구현: Mathf.Max(0, 쿨다운 종료 시각 - 현재 시각)
        //   [TODO: 결정 필요] 시계: 로컬 Time.timeAsDouble (간단, 표시 오차 = 핑) vs FishNet TimeManager 틱 (서버와 일치)
        public float CooldownRemaining => default;

        public bool IsOnCooldown => CooldownRemaining > 0f;

        /// <summary>채널링/지속 효과 진행 중.</summary>
        public virtual bool IsActive => false;

        // 네트워크 권한 체크 지점 — 하위 클래스는 Core.IsServerInitialized / Core.IsOwner 를 직접 쓰지 말고 이걸 쓴다.
        protected bool IsServer => Core != null && Core.IsServerInitialized;
        protected bool IsOwner => Core != null && Core.IsOwner;

        // 호출: AbilityController.Initialize (서버·모든 클라)
        // 구현: Core = core; core.Definition.TryGetAbilityInfo(slot, out var info) 이면 Info = info, 아니면 LogWarning; 쿨다운 리셋
        //   하위 클래스: base.Initialize(core) 를 먼저 부르고 필요한 참조(core.Movement 등)를 캐시.
        public virtual void Initialize(CharacterCore core) { }

        // [흐름②] 소유 클라. 구현: !IsOnCooldown && !Core.Health.IsDead && 능력별 조건(지면, 자원)
        public virtual bool CanActivateLocal(in AbilityContext ctx) { return default; }

        // [흐름②] 소유 클라, 서버 응답 전. 즉시 반응이 필요한 능력만 오버라이드 (대시 이동, 시전 모션).
        //   예측 연출을 했다면 플래그를 남겨 OnObserversActivated 에서 같은 연출을 두 번 재생하지 않게 한다.
        public virtual void OnOwnerPredict(in AbilityContext ctx) { }

        // [흐름③] 서버. 클라가 보낸 ctx 는 조작될 수 있다 (보안 경계).
        // 구현: 서버 기준 쿨다운, 사망/스턴, 자원, ctx.direction 정규화, ctx.origin 이 서버 소켓 위치와 허용 오차 내인지, 사거리
        public virtual bool ServerCanActivate(in AbilityContext ctx) { return default; }

        // [흐름④] 서버. 게임플레이 결과만 (데미지·투사체 스폰·버프·이동). 쿨다운 시작.
        public abstract void ServerActivate(in AbilityContext ctx);

        // [흐름⑤] 모든 클라(소유자·호스트 포함). 연출만. 클라 쪽 쿨다운 표시 시작은 AbilityController 가 StartCooldown 으로.
        public virtual void OnObserversActivated(in AbilityContext ctx) { }

        // [흐름③'] 소유 클라. 서버가 거절함 → OnOwnerPredict 에서 한 것 되돌리기, 쿨다운 표시 원복.
        public virtual void OnActivationRejected() { }

        // 서버. 채널링 중단, 사망 시 정리. 구현: 지속 효과 해제, 이동 잠금 해제 등
        public virtual void ServerCancel() { }

        // 모든 클라. 구현: 루프 VFX 끄기, Core.AnimatorBridge.SetChanneling(false)
        public virtual void OnObserversCancelled() { }

        // 호출: AbilityController.Update (서버·모든 클라). 지속형 능력의 만료 체크 등.
        public virtual void Tick(float deltaTime) { }

        /// <summary>쿨다운 시작. 서버(권위)와 클라(표시) 양쪽에서 AbilityController 가 호출.</summary>
        // 구현: 쿨다운 종료 시각 = 현재 시각 + seconds
        public void StartCooldown(float seconds) { }

        // 구현: 쿨다운 종료 시각 = 0 (거절 시 원복, 부활/라운드 리셋)
        public void ResetCooldown() { }
    }

    /// <summary>
    /// 발동 요청 시 클라 → 서버로 보내는 조준 정보. FishNet 이 public 필드 struct 를 자동 직렬화한다.
    /// 서버는 이 값을 그대로 믿지 않는다 (ServerCanActivate 에서 재검증).
    /// </summary>
    public struct AbilityContext
    {
        public Vector3 origin;        // 소유 클라가 본 발사 원점
        public Vector3 direction;     // 조준 방향 (정규화)
        public Vector3 targetPoint;   // 조준점 (레이캐스트 결과)
        public int targetObjectId;    // 지정 대상 NetworkObject.ObjectId, 없으면 -1 (기본값 0 은 유효 ID 일 수 있으니 반드시 채울 것)
    }
}
