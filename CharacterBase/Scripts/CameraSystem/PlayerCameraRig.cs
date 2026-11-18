using Unity.Cinemachine;
using UnityEngine;

namespace Game.CameraSystem
{
    /// <summary>
    /// 씬에 하나 두는 플레이어 카메라. 로컬 플레이어가 스폰되면 LocalPlayerBinder 가 SetTarget 으로 추적 대상(CameraTarget)을 넘긴다.
    /// 씬 구성:
    ///   Main Camera            : Camera + CinemachineBrain (+ AudioListener)
    ///   PlayerFollowCamera     : CinemachineCamera + 이 컴포넌트 + Position/Rotation Control + CinemachineInputAxisController
    /// [TODO: 결정 필요] 카메라 방식
    ///   OrbitalFollow + RotationComposer + InputAxisController : 마우스 궤도 회전을 Cinemachine 이 처리 (코드 불필요)
    ///   ThirdPersonFollow(숄더뷰)                               : CameraTarget 의 회전을 코드가 마우스로 돌려야 함
    ///   이 스크립트는 어느 쪽이든 그대로 동작한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class PlayerCameraRig : MonoBehaviour
    {
        public static PlayerCameraRig Instance { get; private set; }

        private CinemachineCamera _camera;

        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PlayerCameraRig] 씬에 두 개 이상 있습니다. 이것은 끕니다.", this);
                gameObject.SetActive(false);
                return;
            }
            Instance = this;
            _camera = GetComponent<CinemachineCamera>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetTarget(Transform target)
        {
            CurrentTarget = target;
            _camera.Follow = target;
            _camera.LookAt = target;
            // 리스폰/교체 때 이전 위치에서 날아오지 않고 바로 붙도록 댐핑 상태를 버린다.
            _camera.PreviousStateIsValid = false;
        }

        /// <summary>target 을 따라가고 있을 때만 해제 (이미 다른 캐릭터로 바뀌었으면 무시).</summary>
        public void ClearTarget(Transform target)
        {
            if (CurrentTarget == target) SetTarget(null);
        }
    }
}
