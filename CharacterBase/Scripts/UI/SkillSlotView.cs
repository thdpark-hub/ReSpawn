using Game.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>HUD 스킬 슬롯 1칸. PlayerHUD 가 매 프레임 Show 를 부른다.</summary>
    public sealed class SkillSlotView : MonoBehaviour
    {
        [SerializeField] private AbilitySlot slot;
        [SerializeField] private Image icon;
        [Tooltip("Image Type = Filled, Radial 360. 남은 쿨다운 비율만큼 덮는다.")]
        [SerializeField] private Image cooldownMask;
        [SerializeField] private TMP_Text cooldownText;

        private int _shownSeconds = -1;

        public AbilitySlot Slot => slot;

        /// <summary>ability 가 null 이면 빈 슬롯으로 표시.</summary>
        public void Show(CharacterAbility ability)
        {
            Sprite sprite = ability != null ? ability.Icon : null;
            icon.sprite = sprite;              // Image 는 같은 값이면 다시 그리지 않는다
            icon.enabled = sprite != null;

            float duration = ability != null ? ability.CooldownDuration : 0f;
            float remaining = ability != null ? ability.CooldownRemaining : 0f;
            bool cooling = duration > 0f && remaining > 0f;
            cooldownMask.fillAmount = cooling ? Mathf.Clamp01(remaining / duration) : 0f;

            // 텍스트는 정수 초가 바뀔 때만 갱신 (매 프레임 문자열 할당 방지)
            int seconds = cooling ? Mathf.CeilToInt(remaining) : 0;
            if (seconds == _shownSeconds) return;
            _shownSeconds = seconds;
            cooldownText.text = seconds > 0 ? seconds.ToString() : string.Empty;
        }
    }
}
