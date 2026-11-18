using System;
using FishNet.Object;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 캐릭터 1명을 정의하는 데이터 자산. "캐릭터마다 다른 것"만 담는다 (외형·애니메이션·스탯·능력 메타데이터).
    /// 로직은 없다. 공통 프리팹(PF_Character_Base)의 Variant 가 이 자산 하나만 바꿔 끼우면 다른 캐릭터가 된다.
    /// 생성: Project 창 > Create > Game/Character Definition
    /// 읽는 곳: CharacterCore(인스펙터 참조), CharacterVisualLoader(모델·애니), CharacterMovement/Health(스탯),
    ///          CharacterAbility(능력 메타데이터), PlayerHUD(아이콘·이름), 스포너(networkPrefab)
    /// </summary>
    [CreateAssetMenu(fileName = "CD_NewCharacter", menuName = "Game/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("식별")]
        [Tooltip("네트워크로 주고받는 정수 ID. 프로젝트 전체에서 고유해야 한다.")]
        // [TODO: 결정 필요] int vs string(GUID). 동기화 비용은 int 가 유리. 고유성 검사 위치(에디터 메뉴/DB 자산)도 결정.
        [SerializeField] private int characterId;
        [SerializeField] private string displayName;

        [Header("외형")]
        [Tooltip("루트에 Animator 가 있는 모델 프리팹. FBX 를 감싼 프리팹 권장 (본 아래 Socket_* 마커 포함).")]
        [SerializeField] private GameObject modelPrefab;
        [Tooltip("베이스 = AC_Character_Base. 클립만 교체한다.")]
        [SerializeField] private AnimatorOverrideController animatorOverride;
        [Tooltip("비우면 모델 프리팹 Animator 에 이미 들어 있는 Avatar 를 쓴다.")]
        [SerializeField] private Avatar avatar;

        [Header("UI")]
        [SerializeField] private Sprite icon;

        [Header("스탯")]
        [SerializeField] private CharacterStats baseStats = CharacterStats.Default;

        [Header("특수 기능 메타데이터 (동작 코드는 Variant 의 Abilities 자식 컴포넌트)")]
        [SerializeField] private AbilityInfo[] abilities = Array.Empty<AbilityInfo>();

        [Header("네트워크 (접근 A: 캐릭터별 Variant 를 네트워크 프리팹으로 등록)")]
        [Tooltip("서버가 이 캐릭터로 스폰할 때 Instantiate 할 Variant 프리팹.")]
        [SerializeField] private NetworkObject networkPrefab;

        public int CharacterId => characterId;
        public string DisplayName => displayName;
        public GameObject ModelPrefab => modelPrefab;
        public AnimatorOverrideController AnimatorOverride => animatorOverride;
        public Avatar Avatar => avatar;
        public Sprite Icon => icon;
        public CharacterStats BaseStats => baseStats;
        public AbilityInfo[] Abilities => abilities;
        public NetworkObject NetworkPrefab => networkPrefab;

        /// <summary>슬롯에 해당하는 능력 메타데이터를 찾는다.</summary>
        // 호출: CharacterAbility.Initialize (자기 슬롯의 아이콘·쿨다운 읽기), 캐릭터 선택 UI
        // 구현: abilities 에서 slot 이 같은 항목을 찾아 info 로 내보내고 true. 없으면 info = default, false.
        public bool TryGetAbilityInfo(AbilitySlot slot, out AbilityInfo info)
        {
            info = default;
            return default;
        }

        // 에디터 전용 검증. 자산 값이 바뀔 때마다 Unity 가 호출.
        // 구현: modelPrefab 루트에 Animator 가 있는지, animatorOverride.runtimeAnimatorController 가 공통 베이스인지,
        //       abilities 의 slot 중복, baseStats 값이 0 보다 큰지 검사해 Debug.LogWarning(메시지, this).
        //       networkPrefab 의 CharacterCore.Definition 이 this 인지 (서로 가리키는지) 도 확인.
        private void OnValidate() { }
    }

    /// <summary>캐릭터 기본 스탯. 런타임 버프/장비 보정은 여기 넣지 말고 별도 계층에서 합산한다.</summary>
    [Serializable]
    public struct CharacterStats
    {
        public float maxHealth;
        [Tooltip("m/s, 걷기 기준")] public float moveSpeed;
        public float sprintMultiplier;
        [Tooltip("m")] public float jumpHeight;
        [Tooltip("deg/s")] public float turnSpeed;
        // [TODO: 결정 필요] 공격력/방어력/자원(마나·스태미나) 등 게임 규칙이 정해지면 추가

        // 새 자산 기본값 (임시 수치)
        public static CharacterStats Default => new CharacterStats
        {
            maxHealth = 100f, moveSpeed = 4f, sprintMultiplier = 1.5f, jumpHeight = 1.2f, turnSpeed = 720f,
        };
    }

    /// <summary>능력 1개의 표시·튜닝 데이터. 동작은 같은 slot 을 가진 CharacterAbility 컴포넌트가 한다.</summary>
    [Serializable]
    public struct AbilityInfo
    {
        public AbilitySlot slot;
        public string displayName;
        public Sprite icon;
        [Tooltip("초")] public float cooldown;
        [TextArea] public string description;
    }
}
