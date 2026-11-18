using Game.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 머리 위 이름표 + 체력바 (World Space Canvas). 프리팹의 Root/NameplateAnchor/Nameplate 에 둔다.
    /// 모든 클라이언트에서 모든 캐릭터에 대해 동작한다. 자기 캐릭터의 이름표는 기본으로 숨긴다 (HUD 가 대신 보여 줌).
    /// 이름이 아직 안 왔으면(빈 문자열) 캐릭터 표시 이름으로 대신한다.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class CharacterNameplate : MonoBehaviour
    {
        [Tooltip("비우면 부모에서 찾는다")]
        [SerializeField] private CharacterCore core;
        [SerializeField] private TMP_Text nameText;
        [Tooltip("Image Type = Filled, Horizontal")]
        [SerializeField] private Image healthFill;
        [SerializeField] private bool hideForLocalOwner = true;

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            if (core == null) core = GetComponentInParent<CharacterCore>();
        }

        private void OnEnable()
        {
            core.Health.HealthChanged += OnHealthChanged;
            core.NetworkSync.PlayerNameChanged += OnNameChanged;
            OnHealthChanged(core.Health.Current, core.Health.Max);
            OnNameChanged(core.NetworkSync.PlayerName);
        }

        private void OnDisable()
        {
            core.Health.HealthChanged -= OnHealthChanged;
            core.NetworkSync.PlayerNameChanged -= OnNameChanged;
        }

        private void LateUpdate()
        {
            // 전용 서버(카메라 없음)와 자기 캐릭터에서는 그리지 않는다. 소유권이 바뀌어도 매 프레임 판정이라 따라간다.
            Camera cam = Camera.main;
            bool visible = cam != null && !(hideForLocalOwner && core.IsOwner);
            _canvas.enabled = visible;
            // 화면과 평행하게 (원근 왜곡 없는 빌보드)
            if (visible) transform.rotation = cam.transform.rotation;
        }

        private void OnHealthChanged(float current, float max)
        {
            healthFill.fillAmount = max > 0f ? current / max : 0f;
        }

        private void OnNameChanged(string playerName)
        {
            bool fallback = string.IsNullOrEmpty(playerName) && core.Definition != null;
            nameText.text = fallback ? core.Definition.DisplayName : playerName;
        }
    }
}
