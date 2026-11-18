using FishNet.Connection;
using FishNet.Object;
using Game.CameraSystem;
using Game.UI;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 이 캐릭터가 "내 것"(로컬 소유자)일 때만 카메라·HUD·입력을 연결하고, 소유권을 잃거나 디스폰되면 해제한다.
    /// 전용 서버와 남의 캐릭터에서는 아무것도 하지 않는다. 호스트의 자기 캐릭터는 정상적으로 연결된다.
    /// FishNet 은 스폰 시 OnStartClient 다음에 OnOwnershipClient 를 항상 한 번 부르므로, 시작·소유권 변경을 이 콜백 하나로 처리한다.
    /// </summary>
    [RequireComponent(typeof(CharacterCore))]
    public sealed class LocalPlayerBinder : NetworkBehaviour
    {
        [SerializeField] private CharacterCore core;

        private bool _bound;

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            if (IsOwner) Bind();
            else Unbind();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            Unbind();
        }

        private void Bind()
        {
            if (_bound) return;
            _bound = true;

            if (PlayerCameraRig.Instance != null) PlayerCameraRig.Instance.SetTarget(core.CameraTarget);
            else Debug.LogWarning("[LocalPlayerBinder] 씬에 PlayerCameraRig 가 없어 카메라를 연결하지 못했습니다.", this);

            if (PlayerHUD.Instance != null) PlayerHUD.Instance.Bind(core);
            else Debug.LogWarning("[LocalPlayerBinder] 씬에 PlayerHUD 가 없어 HUD 를 연결하지 못했습니다.", this);

            core.Input.enabled = true;
        }

        private void Unbind()
        {
            if (!_bound) return;
            _bound = false;

            core.Input.enabled = false;
            // 씬 언로드 중이면 HUD/카메라가 먼저 파괴됐을 수 있다.
            if (PlayerHUD.Instance != null) PlayerHUD.Instance.Unbind(core);
            if (PlayerCameraRig.Instance != null) PlayerCameraRig.Instance.ClearTarget(core.CameraTarget);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (core == null) core = GetComponent<CharacterCore>();
        }
    }
}
