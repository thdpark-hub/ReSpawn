using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// Definition 의 모델을 Visual 슬롯에 두고 Animator(컨트롤러·아바타)와 소켓을 연결한다.
    /// 네트워크 코드 없음 → 서버/클라/오프라인 공통.
    ///
    /// 모델이 오는 경로 (Build 가 둘 다 처리하도록 구현):
    ///   - 접근 A(권장): Variant 의 Visual 아래에 모델을 미리 넣어 둔다 → Build 는 "있는 모델 연결"만.
    ///     에디터에서 소켓·콜라이더 위치를 눈으로 맞출 수 있다.
    ///   - 접근 B: Visual 이 비어 있으면 definition.ModelPrefab 을 Instantiate.
    /// </summary>
    public sealed class CharacterVisualLoader : MonoBehaviour
    {
        [Tooltip("Root/Visual")]
        [SerializeField] private Transform visualSlot;
        [Tooltip("Root/Sockets (루트 기준 소켓들, 자식 이름 = SocketId 이름)")]
        [SerializeField] private Transform rootSockets;
        [Tooltip("모델 프리팹 안 본 소켓 마커의 이름 접두어. 예: Socket_RightHand")]
        [SerializeField] private string boneSocketPrefix = "Socket_";

        private readonly Dictionary<SocketId, Transform> _sockets = new Dictionary<SocketId, Transform>();

        public GameObject CurrentModel { get; private set; }
        public Animator Animator { get; private set; }

        /// <summary>모델이 바뀐 직후. 접근 B 에서 CharacterAnimatorBridge / NetworkAnimator 재연결용.</summary>
        public event Action<Animator> VisualChanged;

        // 호출: CharacterCore.Awake (접근 A) / CharacterCore.ApplyDefinition (접근 B)
        // 구현:
        //   1) visualSlot 에 자식이 있으면 첫 자식을 CurrentModel 로 사용,
        //      없으면 Instantiate(definition.ModelPrefab, visualSlot, false)
        //      (교체면 기존 모델을 먼저 Destroy — Destroy 는 프레임 끝에 일어나므로 바로 GetComponentInChildren 하면 옛 모델이 잡힐 수 있음)
        //   2) Animator = CurrentModel.GetComponent<Animator>()  — 모델 "루트"에 있어야 한다. 없으면 LogError.
        //   3) SetupAnimator(Animator, definition)
        //   4) 모델에 딸려 온 Collider/Rigidbody 제거 또는 비활성 (CharacterController·Hurtbox 와 겹침 방지)
        //   5) BindSockets()
        //   6) VisualChanged?.Invoke(Animator)
        public void Build(CharacterDefinition definition) { }

        // 구현:
        //   animator.runtimeAnimatorController = def.AnimatorOverride;
        //   if (def.Avatar != null) animator.avatar = def.Avatar;
        //   animator.applyRootMotion = false;   (루트모션은 Relay → Movement 가 필요할 때만 직접 적용)
        //   animator.cullingMode = 클라: CullUpdateTransforms
        //       [TODO: 결정 필요] 전용 서버: 본 위치/애니 이벤트에 의존하지 않는 설계면 Animator 를 꺼서 CPU 절약,
        //       의존한다면 AlwaysAnimate (화면이 없어 컬링되면 이벤트·본이 멈춤)
        //   animator.Rebind(); animator.Update(0f);  (컨트롤러/아바타 교체 직후 한 프레임 T-포즈 방지)
        private void SetupAnimator(Animator animator, CharacterDefinition def) { }

        // 구현: SocketId 값마다 한 번씩 (Enum.GetValues)
        //   1) CurrentModel 아래에서 boneSocketPrefix + id 이름 Transform 탐색 (모델 프리팹의 본 아래 캐릭터별 마커)
        //   2) 없으면 rootSockets.Find(id.ToString())  (루트 기준 공통 위치)
        //   3) 둘 다 없으면 transform 으로 대체하고 LogWarning
        //   결과를 _sockets 에 캐시. 이름 검색은 Build 때만 (매 프레임 Find 금지).
        //   주의: 모델 Rig 의 "Optimize Game Objects" 를 켜면 본 Transform 이 숨겨진다 → 마커가 달린 본을 "Extra Transforms to Expose" 에 추가.
        private void BindSockets() { }

        /// <summary>소켓 Transform. 능력(투사체 발사 위치)·이펙트 부착·무기 장착에서 사용.</summary>
        // 구현: _sockets 에서 찾아 반환, 없으면 transform.
        public Transform GetSocket(SocketId id) { return default; }

        // 호출: 접근 B 의 교체 / 디스폰 정리. 구현: CurrentModel Destroy, Animator = null, _sockets.Clear()
        public void Clear() { }
    }

    /// <summary>
    /// 소켓 종류.
    /// 루트 소켓: Root/Sockets 아래. 애니메이션과 무관하게 고정 → 서버 판정(발사 원점, 범위 중심)은 여기만 쓴다.
    ///           (전용 서버는 애니메이션을 안 돌리거나 컬링할 수 있어 본 위치를 믿을 수 없다)
    /// 본 소켓  : 모델 프리팹의 본 아래 Socket_* 마커. 애니메이션을 따라감 → 연출(무기 쥐기, 손 이펙트) 전용.
    /// </summary>
    public enum SocketId
    {
        // 루트 소켓
        Center,
        Overhead,
        ProjectileOrigin,
        Feet,
        // 본 소켓
        RightHand,
        LeftHand,
        Head,
    }
}
