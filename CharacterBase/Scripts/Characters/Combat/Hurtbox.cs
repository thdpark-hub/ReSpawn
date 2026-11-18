using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 피격 영역. Root/Combat/Hurtbox 의 트리거 콜라이더(Layer = Hurtbox)에 붙는 데이터 홀더.
    /// 공격 쪽(Hitbox, 투사체, 범위기)은 물리 쿼리로 Hurtbox 레이어만 찾고 → hurtbox.Core.Health 로 데미지를 넣는다.
    /// CharacterController 캡슐과 분리하는 이유: 이동 충돌 크기와 피격 크기·레이어를 독립적으로 조정하기 위해.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Hurtbox : MonoBehaviour
    {
        [SerializeField] private CharacterCore core;
        // [TODO: 결정 필요] 부위 판정(머리 1.5배 등)을 할지. 하면 본에 붙은 Hurtbox 가 필요한데,
        //   전용 서버가 애니메이션을 안 돌리면 본 위치가 틀리므로 서버 Animator 를 AlwaysAnimate 로 둬야 한다.
        [SerializeField] private float damageMultiplier = 1f;

        public CharacterCore Core => core;
        public float DamageMultiplier => damageMultiplier;

        // 에디터에서 컴포넌트를 처음 붙일 때. 구현: GetComponent<Collider>().isTrigger = true; core = GetComponentInParent<CharacterCore>();
        private void Reset() { }
    }
}
