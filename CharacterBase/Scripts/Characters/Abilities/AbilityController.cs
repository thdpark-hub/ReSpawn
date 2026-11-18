using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>입력 슬롯. Animator 의 SkillIndex = (int)slot + 1 (0 은 기본 공격용으로 비워 둠).</summary>
    public enum AbilitySlot : byte
    {
        Skill1 = 0,
        Skill2 = 1,
        Ultimate = 2,
    }

    /// <summary>
    /// Root/Abilities 아래의 CharacterAbility 를 모아 슬롯(Skill1/Skill2/Ultimate)에 매핑하고,
    /// 발동의 네트워크 흐름을 한 곳에서 처리한다. 능력 쪽은 RPC 를 몰라도 된다.
    ///
    /// 발동 흐름 (각 메서드 주석에 같은 번호로 표시):
    ///   ①  [소유 클라] CharacterInput.Update            키 입력 → TryActivate(slot, ctx)
    ///   ②  [소유 클라] TryActivate                      CanActivateLocal → (선택) OnOwnerPredict → ServerRequestActivate RPC
    ///   ③  [서버]      ServerRequestActivate            소유권(FishNet 자동) · 슬롯 · 공통 상태 · ServerCanActivate 검증
    ///   ③' [서버→소유] TargetActivationRejected          실패 시 → OnActivationRejected (예측 되돌리기)
    ///   ④  [서버]      ability.ServerActivate           게임플레이 결과 적용, 서버 쿨다운 시작
    ///   ⑤  [서버→전체] ObserversActivated               클라 쿨다운 표시 + 애니 트리거 + OnObserversActivated 연출
    ///   ⑥  [상태 동기화] 결과의 "지속 상태"는 RPC 가 아니라 SyncVar/NetworkObject 로 퍼진다
    ///                   (체력 = CharacterHealth SyncVar, 투사체 = 스폰된 NetworkObject, 위치 = NetworkTransform/예측)
    ///                   → 늦게 접속한 클라도 올바른 상태를 본다.
    /// 호스트: ②의 ServerRpc 가 같은 프로세스에서 즉시 실행되고 ⑤도 같은 프레임에 온다 → 예측 연출과 ⑤ 연출이 겹치지 않게 주의.
    /// </summary>
    public sealed class AbilityController : NetworkBehaviour
    {
        public const int SlotCount = 3;

        [Tooltip("Root/Abilities")]
        [SerializeField] private Transform abilitiesContainer;

        private readonly CharacterAbility[] _bySlot = new CharacterAbility[SlotCount];
        private CharacterCore _core;

        /// <summary>슬롯의 능력. 비어 있으면 null. PlayerHUD 가 매 프레임 읽는다.</summary>
        public CharacterAbility GetAbility(AbilitySlot slot) => _bySlot[(int)slot];

        // 호출: CharacterCore.OnStartNetwork (서버·모든 클라 — 클라도 쿨다운 표시·연출에 인스턴스가 필요)
        // 구현:
        //   _core = core;
        //   abilitiesContainer.GetComponentsInChildren<CharacterAbility>(true) 순회
        //     → 같은 슬롯이 이미 차 있으면 LogError (Variant 설정 실수) → _bySlot[(int)a.Slot] = a → a.Initialize(core)
        //   if (IsServerInitialized) core.Health.Died += ServerCancelAll;
        public void Initialize(CharacterCore core) { }

        /// <summary>[흐름②] 소유 클라에서 발동 시도. 호출: CharacterInput.</summary>
        // 구현:
        //   if (!IsOwner) return;
        //   var a = GetAbility(slot); if (a == null || !a.CanActivateLocal(ctx)) return;   — 쿨다운 중 연타는 서버로 보내지 않음
        //   a.OnOwnerPredict(ctx);
        //   ServerRequestActivate(slot, ctx);
        public void TryActivate(AbilitySlot slot, AbilityContext ctx) { }

        // [흐름③④⑤] 서버. ServerRpc 기본값 RequireOwnership = true → 소유자가 아닌 연결의 호출은 FishNet 이 버린다.
        // 구현 (slot·ctx 는 클라가 보낸 값 = 신뢰 경계):
        //   ③ (byte)slot < SlotCount, 능력 존재, !_core.Health.IsDead, (스턴 등 공통 상태), a.ServerCanActivate(ctx)
        //      실패 → TargetActivationRejected(Owner, slot); return;
        //   ④ a.ServerActivate(ctx); a.StartCooldown(a.CooldownDuration);
        //   ⑤ ObserversActivated(slot, ctx);
        [ServerRpc]
        private void ServerRequestActivate(AbilitySlot slot, AbilityContext ctx) { }

        // [흐름⑤] 모든 클라 (소유자·호스트 포함).
        // 구현: var a = GetAbility(slot);
        //       if (!IsServerInitialized) a.StartCooldown(a.CooldownDuration);   — 호스트는 ④에서 이미 시작
        //       _core.AnimatorBridge.PlaySkill(slot); a.OnObserversActivated(ctx);
        //   [TODO: 결정 필요] 소유자 예측 연출과의 중복: 능력 쪽 플래그로 거르기 vs [ObserversRpc(ExcludeOwner = true)] 로 소유자 제외
        //   이 RPC 는 늦게 접속한 클라에게 재전송되지 않는다 (일회성 연출 전용).
        [ObserversRpc]
        private void ObserversActivated(AbilitySlot slot, AbilityContext ctx) { }

        // [흐름③'] 서버 → 해당 소유 클라만. 구현: GetAbility(slot).OnActivationRejected(); 쿨다운 표시 원복
        [TargetRpc]
        private void TargetActivationRejected(NetworkConnection conn, AbilitySlot slot) { }

        /// <summary>채널링/지속 능력 수동 취소 요청. 소유 클라. 구현: if (!IsOwner) return; ServerRequestCancel(slot);</summary>
        public void TryCancel(AbilitySlot slot) { }

        // 서버. 구현: (byte)slot < SlotCount 검증 → ServerCancel(slot);
        [ServerRpc]
        private void ServerRequestCancel(AbilitySlot slot) { }

        // 모든 클라. 구현: GetAbility(slot).OnObserversCancelled();
        [ObserversRpc]
        private void ObserversCancelled(AbilitySlot slot) { }

        /// <summary>서버 전용. 한 슬롯을 끝낸다. 지속형 능력이 시간이 다 돼 스스로 끝날 때도 이것을 부른다 (능력은 RPC 를 직접 못 쏘므로).</summary>
        // 구현: a.IsActive 일 때만 a.ServerCancel(); ObserversCancelled(slot);
        public void ServerCancel(AbilitySlot slot) { }

        /// <summary>서버 전용. 사망·스턴·라운드 종료 시 진행 중인 능력을 모두 끊는다.</summary>
        // 구현: 모든 슬롯에 ServerCancel(slot)
        public void ServerCancelAll() { }

        // 구현: _bySlot 의 null 아닌 능력마다 Tick(Time.deltaTime)
        //   [TODO: 결정 필요] 예측 이동을 쓰면 TimeManager.OnTick 으로 옮겨 틱 단위로
        private void Update() { }
    }
}
