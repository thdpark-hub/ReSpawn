using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 모델 루트(Animator 와 같은 GameObject)에 붙는 얇은 중계기. CharacterAnimatorBridge.Bind 가 런타임에 자동으로 붙인다.
    /// 모델 프리팹에 미리 붙여 둘 필요 없음. 로직 금지 — 받은 것을 Bridge 로 넘기기만 한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimatorEventRelay : MonoBehaviour
    {
        private CharacterAnimatorBridge _bridge;
        private Animator _animator;

        // 호출: CharacterAnimatorBridge.Bind. 구현: _bridge = bridge; _animator = GetComponent<Animator>();
        public void Bind(CharacterAnimatorBridge bridge) { }

        // Unity 콜백. 이 메서드가 "있는 것만으로" Animator 의 Apply Root Motion 이 "Handled by Script" 가 되어
        // Unity 가 루트모션을 모델 Transform 에 자동 적용하지 않는다 → 모델이 루트에서 혼자 걸어나가는 문제 방지.
        // 구현: if (_bridge != null) _bridge.ReceiveRootMotion(_animator.deltaPosition, _animator.deltaRotation);
        private void OnAnimatorMove() { }

        // 클립의 Animation Event: Function = "AnimEvent", String = AnimEvents.* 값.
        // 이벤트 함수를 하나로 통일 → 모델/클립이 바뀌어도 로직 쪽 메서드명을 건드릴 일이 없다.
        // 구현: if (_bridge != null) _bridge.ReceiveAnimationEvent(e.stringParameter);
        // 주의: 받는 함수가 없는 이벤트는 "has no receiver" 에러를 낸다 → 외부 에셋 클립의 기존 이벤트(FootL 등)는
        //       임포트 설정에서 지우거나 Function 을 AnimEvent 로 바꾼다.
        public void AnimEvent(AnimationEvent e) { }
    }
}
