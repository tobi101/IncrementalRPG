using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Entity
{
    public class CreatureView : MonoBehaviour
    {
        private static readonly int GetHitTrigger = Animator.StringToHash("Get Hit");
        private const string IdleAnimationName = "idle";
        private const string DamageAnimationName = "damage";
        private const string DeathAnimationName = "explosion";

        [SerializeField] private Transform _footAnchor;
        [SerializeField] private SkeletonAnimation _animationBody;
        [SerializeField] private SkeletonAnimation _deathAnimationBody;

        public Vector3 FootOffset => _footAnchor != null
            ? transform.position - _footAnchor.position
            : Vector3.zero;

        private Creature _bound;
        private int _previousHealth;
        private TrackEntry _deathTrackEntry;
        private Action _deathCompleteCallback;

        public void Bind(Creature creature)
        {
            _bound = creature;
            _previousHealth = creature.CurrentHP;
            _bound.OnHealthChanged += HandleHealthChanged;

            ResetAnimationBodies();

            if (_animationBody != null)
                _animationBody.AnimationState.SetAnimation(0, IdleAnimationName, true);
        }

        public void Unbind()
        {
            if (_bound == null) return;
            _bound.OnHealthChanged -= HandleHealthChanged;
            _bound = null;
        }

        public void ResetForPool()
        {
            Unbind();
            ResetAnimationBodies();
        }

        public void PlayDeath(Action onComplete)
        {
            Unbind();
            ClearDeathCallback();

            if (_animationBody != null)
                _animationBody.gameObject.SetActive(false);

            if (_deathAnimationBody == null)
            {
                onComplete?.Invoke();
                return;
            }

            _deathAnimationBody.gameObject.SetActive(true);
            _deathAnimationBody.AnimationState.ClearTracks();
            _deathAnimationBody.Skeleton.SetToSetupPose();

            if (_deathAnimationBody.Skeleton.Data.FindAnimation(DeathAnimationName) == null)
            {
                Debug.LogWarning($"[CreatureView] Death animation '{DeathAnimationName}' is missing on '{name}'.");
                onComplete?.Invoke();
                return;
            }

            _deathCompleteCallback = onComplete;
            _deathTrackEntry = _deathAnimationBody.AnimationState.SetAnimation(0, DeathAnimationName, false);
            _deathTrackEntry.Complete += HandleDeathAnimationComplete;
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (_animationBody != null && current < _previousHealth)
                _animationBody.AnimationState.SetAnimation(0, DamageAnimationName, false);

            _previousHealth = current;
        }

        private void ResetAnimationBodies()
        {
            ClearDeathCallback();

            if (_animationBody != null)
                _animationBody.gameObject.SetActive(true);

            if (_deathAnimationBody != null)
            {
                _deathAnimationBody.AnimationState.ClearTracks();
                _deathAnimationBody.Skeleton.SetToSetupPose();
                _deathAnimationBody.gameObject.SetActive(false);
            }
        }

        private void HandleDeathAnimationComplete(TrackEntry trackEntry)
        {
            if (trackEntry != _deathTrackEntry) return;

            var callback = _deathCompleteCallback;
            ClearDeathCallback();
            callback?.Invoke();
        }

        private void ClearDeathCallback()
        {
            if (_deathTrackEntry != null)
                _deathTrackEntry.Complete -= HandleDeathAnimationComplete;

            _deathTrackEntry = null;
            _deathCompleteCallback = null;
        }
    }
}
