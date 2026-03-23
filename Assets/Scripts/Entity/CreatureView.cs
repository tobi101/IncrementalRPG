using UnityEngine;

namespace Entity
{
    public class CreatureView : MonoBehaviour
    {
        [SerializeField] private Transform _footAnchor;

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
            // TODO: update health bar visual
        }
    }
}
