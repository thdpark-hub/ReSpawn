using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 체력. 값은 서버만 바꾼다 (Server* 메서드). 클라이언트는 SyncVar 변경 콜백으로 UI·연출만 한다.
    /// 늦게 접속한 클라도 SyncVar 로 현재 체력·사망 상태를 받는다 (RPC 는 버퍼되지 않음).
    /// 확장: 실드/무적/감소 버프는 IDamageModifier 를 구현한 컴포넌트를 캐릭터 아래에 추가하면 자동 적용 (이 파일 수정 불필요).
    /// </summary>
    public sealed class CharacterHealth : NetworkBehaviour
    {
        // 서버 → 모든 클라. .Value 쓰기는 서버에서만.
        private readonly SyncVar<float> _current = new SyncVar<float>();
        private readonly SyncVar<float> _max = new SyncVar<float>();
        private readonly SyncVar<bool> _isDead = new SyncVar<bool>();

        private CharacterCore _core;
        private IDamageModifier[] _modifiers = Array.Empty<IDamageModifier>();

        public float Current => _current.Value;
        public float Max => _max.Value;
        public bool IsDead => _isDead.Value;

        /// <summary>(현재, 최대). PlayerHUD / CharacterNameplate 가 구독. 클라이언트 쪽에서 발생시킨다.</summary>
        public event Action<float, float> HealthChanged;
        /// <summary>피격 연출용 (피격 모션, 데미지 숫자). 클라이언트에서 발생.</summary>
        public event Action<DamageInfo> Damaged;
        public event Action Died;
        public event Action Revived;

        // 호출: CharacterCore.OnStartNetwork
        // 구현:
        //   _core = core;
        //   _modifiers = core.GetComponentsInChildren<IDamageModifier>(true);  (Abilities 아래 실드 등)
        //   _current.OnChange += OnCurrentChanged; _max.OnChange += ...; _isDead.OnChange += OnDeadChanged;
        //   if (IsServerInitialized) { _max.Value = core.Definition.BaseStats.maxHealth; _current.Value = _max.Value; _isDead.Value = false; }
        public void Initialize(CharacterCore core) { }

        /// <summary>데미지 적용. 실제로 깎인 양을 반환. 서버 전용.</summary>
        // 호출: Hitbox, 투사체, 범위기 (모두 서버에서 판정한 뒤)
        // 구현:
        //   if (!IsServerInitialized || IsDead) return 0;
        //   foreach (m in _modifiers) m.ModifyIncomingDamage(ref info);   — 실드·무적
        //   깎기 → _current.Value = Mathf.Max(0, ...)
        //   ObserversDamaged(info);   — 연출 브로드캐스트
        //   0 이 되면 ServerKill(info)
        public float ServerApplyDamage(DamageInfo info) { return default; }

        // 구현: 서버 전용. 사망 상태면 무시. _current.Value = Mathf.Min(_max.Value, _current.Value + amount)
        public void ServerHeal(float amount) { }

        // 구현: 서버 전용. _current.Value = 0; _isDead.Value = true; _core.Movement.AddMoveLock(); _core.Abilities.ServerCancelAll();
        //       킬 기록/리스폰 예약은 게임 모드 쪽 이벤트로 넘긴다 (여기서 직접 하지 않음).
        public void ServerKill(DamageInfo lastHit) { }

        // 구현: 서버 전용. _isDead.Value = false; _current.Value = _max.Value * healthRatio; _core.Movement.RemoveMoveLock();
        public void ServerRevive(float healthRatio) { }

        // 호출: FishNet SyncVar 변경 (서버·클라).
        // 구현: if (asServer) return;  — 호스트는 서버/클라 양쪽으로 불릴 수 있으므로 UI 이벤트는 클라 쪽에서만 발행
        //       HealthChanged?.Invoke(next, _max.Value);
        //   주의: 전용 서버에는 UI 가 없으므로 서버 로직은 이 콜백에 의존하지 않는다 (Server* 메서드 안에서 처리).
        private void OnCurrentChanged(float prev, float next, bool asServer) { }

        // 구현: if (asServer) return; next ? Died : Revived 발행; _core.AnimatorBridge.SetDead(next);
        private void OnDeadChanged(bool prev, bool next, bool asServer) { }

        // 서버 → 모든 클라. 피격 연출 전달 (체력 수치 자체는 SyncVar 로 따로 온다).
        // 구현: Damaged?.Invoke(info); _core.AnimatorBridge.PlayHit();
        [ObserversRpc]
        private void ObserversDamaged(DamageInfo info) { }
    }

    /// <summary>데미지 1건. FishNet 이 public 필드 struct 를 자동 직렬화한다.</summary>
    public struct DamageInfo
    {
        public float amount;
        public int attackerObjectId;   // 공격자 NetworkObject.ObjectId, 없으면 -1
        public Vector3 hitPoint;
        public Vector3 hitDirection;
        // [TODO: 결정 필요] 데미지 타입(물리/마법), 크리티컬, 넉백 세기
    }

    /// <summary>
    /// 데미지가 체력에 들어가기 직전에 값을 바꾸는 확장 지점 (실드, 무적, 데미지 감소).
    /// 능력 컴포넌트가 이 인터페이스만 구현하면 CharacterHealth 가 Initialize 때 자동으로 수집해 호출한다.
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>서버에서만 호출. info.amount 를 줄이거나 0 으로 만들어 무효화.</summary>
        void ModifyIncomingDamage(ref DamageInfo info);
    }
}
