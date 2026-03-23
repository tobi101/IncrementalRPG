using UnityEngine;

namespace Entity
{
    public class CreatureView : MonoBehaviour
    {
        private static readonly int GetHitTrigger = Animator.StringToHash("Get Hit");

        [SerializeField] private Transform _footAnchor;
        [SerializeField] private Animator _animator;

        public Vector3 FootOffset => _footAnchor != null
            ? transform.position - _footAnchor.position
            : Vector3.zero;

        private Creature _bound;

        public void Bind(Creature creature)
        {
            _bound = creature;
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
            if (_animator == null) return;
            _animator.SetTrigger(GetHitTrigger);
        }
    }
}
