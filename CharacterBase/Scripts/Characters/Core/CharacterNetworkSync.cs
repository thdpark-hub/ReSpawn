using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Game.Characters
{
    /// <summary>
    /// 캐릭터 단위로 동기화할 "정체성" 상태: 플레이어 이름, 캐릭터 ID, 팀.
    /// 체력은 CharacterHealth, 위치는 NetworkTransform/예측, 능력 발동은 AbilityController RPC 가 각각 맡는다.
    /// 여기 값은 모두 서버가 쓰고 SyncVar 로 퍼진다 → 늦게 접속한 클라도 자동으로 받는다.
    /// </summary>
    public sealed class CharacterNetworkSync : NetworkBehaviour
    {
        private readonly SyncVar<string> _playerName = new SyncVar<string>();
        // 접근 A 에선 UI/로그용, 접근 B 에선 외형을 결정하는 핵심 값.
        private readonly SyncVar<int> _characterId = new SyncVar<int>();
        // [TODO: 결정 필요] 팀 개념 유무 (협동이면 전원 0, 대전이면 팀 번호)
        private readonly SyncVar<int> _teamId = new SyncVar<int>();

        private CharacterCore _core;

        public string PlayerName => _playerName.Value;
        public int CharacterId => _characterId.Value;
        public int TeamId => _teamId.Value;

        /// <summary>CharacterNameplate 가 구독. 클라이언트에서 발생.</summary>
        public event Action<string> PlayerNameChanged;
        public event Action<int> CharacterIdChanged;

        // 호출: CharacterCore.OnStartNetwork
        // 구현:
        //   _core = core;
        //   _playerName.OnChange += OnPlayerNameChanged; _characterId.OnChange += OnCharacterIdChanged;
        //   if (IsServerInitialized) _characterId.Value = core.Definition.CharacterId;   (접근 A)
        //   [TODO: 결정 필요] FishNet NetworkAnimator 를 쓰면 여기서 networkAnimator.SetAnimator(core.Visual.Animator)
        //     — 모델/Animator 가 Visual 슬롯 아래 런타임에 정해지므로 프리팹 직렬화 대신 코드로 연결.
        public void Initialize(CharacterCore core) { }

        /// <summary>서버 전용. 스폰 직후 스포너/로비가 연결(NetworkConnection)의 닉네임으로 호출.</summary>
        // 구현: 신뢰 경계 — 이름은 클라가 보낸 문자열이므로 길이 제한(예: 16자)·공백 정리·금칙어 필터 후 _playerName.Value = ...
        public void ServerSetPlayerName(string playerName) { }

        /// <summary>서버 전용. 접근 B 에서 캐릭터 선택/변경 시.</summary>
        // 구현: 존재하는 ID 인지 확인 (Definition DB 조회) 후 _characterId.Value = id;
        public void ServerSetCharacterId(int characterId) { }

        // 구현: 서버 전용. _teamId.Value = teamId;
        public void ServerSetTeam(int teamId) { }

        // 구현: if (asServer) return; PlayerNameChanged?.Invoke(next);
        private void OnPlayerNameChanged(string prev, string next, bool asServer) { }

        // 구현: CharacterIdChanged?.Invoke(next);
        //   접근 B: Definition DB 에서 next 로 조회 → _core.ApplyDefinition(def)  (서버·클라 모두 외형/능력 구성 필요하므로 asServer 무관)
        //   접근 A: 아무것도 안 함
        private void OnCharacterIdChanged(int prev, int next, bool asServer) { }
    }
}
