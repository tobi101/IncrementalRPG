using Spine.Unity;
using UnityEngine;

namespace Entity
{
    public class CreatureView : MonoBehaviour
    {
        private static readonly int GetHitTrigger = Animator.StringToHash("Get Hit");

        [SerializeField] private Transform _footAnchor;
        [SerializeField] private SkeletonAnimation _animationBody;

        public Vector3 FootOffset => _footAnchor != null
            ? transform.position - _footAnchor.position
            : Vector3.zero;

        private Creature _bound;
        private int _previousHealth;

        public void Bind(Creature creature)
        {
            _bound = creature;
            _previousHealth = creature.CurrentHP;
            _bound.OnHealthChanged += HandleHealthChanged;
        }

        public void Unbind()
        {
            if (_bound == null) return;
            _bound.OnHealthChanged -= HandleHealthChanged;
            _bound = null;
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (_animationBody != null && current < _previousHealth)
                _animationBody.AnimationState.SetAnimation(0, "damage", false);

            _previousHealth = current;
        }
    }
}
