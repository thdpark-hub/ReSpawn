using Game.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 화면 HUD (Screen Space Canvas, 씬에 하나). 로컬 플레이어 캐릭터의 체력·초상화·이름·스킬 쿨다운을 표시한다.
    /// 연결은 LocalPlayerBinder 가 한다 — HUD 는 캐릭터를 찾지 않는다.
    /// 이 컴포넌트는 항상 켜진 오브젝트에 두고, 숨길 내용물은 content 자식 패널로 둔다 (꺼진 오브젝트는 Awake 가 안 불려 Instance 가 비게 됨).
    /// </summary>
    public sealed class PlayerHUD : MonoBehaviour
    {
        public static PlayerHUD Instance { get; private set; }

        [Tooltip("바인딩 전엔 숨길 자식 패널")]
        [SerializeField] private GameObject content;
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text characterName;
        [Tooltip("Image Type = Filled, Horizontal")]
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private SkillSlotView[] skillSlots;

        private CharacterCore _core;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PlayerHUD] 씬에 두 개 이상 있습니다. 이것은 끕니다.", this);
                gameObject.SetActive(false);
                return;
            }
            Instance = this;
            content.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Unbind(_core);
            Instance = null;
        }

        public void Bind(CharacterCore core)
        {
            if (core == null || core == _core) return;
            Unbind(_core);
            _core = core;

            CharacterDefinition def = core.Definition;
            portrait.sprite = def != null ? def.Icon : null;
            portrait.enabled = portrait.sprite != null;
            characterName.text = def != null ? def.DisplayName : string.Empty;

            core.Health.HealthChanged += OnHealthChanged;
            OnHealthChanged(core.Health.Current, core.Health.Max);

            content.SetActive(true);
        }

        /// <summary>core 가 현재 연결된 캐릭터일 때만 해제.</summary>
        public void Unbind(CharacterCore core)
        {
            // ReferenceEquals: 이미 파괴된 core 로도 구독 해제는 해야 하므로 Unity 의 == null 판정을 피한다.
            if (ReferenceEquals(core, null) || !ReferenceEquals(core, _core)) return;

            core.Health.HealthChanged -= OnHealthChanged;
            _core = null;
            foreach (SkillSlotView slot in skillSlots) slot.Show(null);
            content.SetActive(false);
        }

        private void Update()
        {
            if (_core == null) return;
            // 쿨다운은 매 프레임 변하므로 이벤트 대신 폴링. 능력 구성이 런타임에 바뀌어도(접근 B) 자동 반영된다.
            foreach (SkillSlotView slot in skillSlots) slot.Show(_core.Abilities.GetAbility(slot.Slot));
        }

        private void OnHealthChanged(float current, float max)
        {
            healthFill.fillAmount = max > 0f ? current / max : 0f;
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}
