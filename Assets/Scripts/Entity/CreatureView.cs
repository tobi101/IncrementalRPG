using System;
using System.Collections.Generic;
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

        [Serializable]
        private sealed class DeathAnimationBody
        {
            [SerializeField] private SkeletonAnimation _animationBody;
            [SerializeField] private string _animationName = DeathAnimationName;
            [SerializeField] private bool _visibleWhileAlive;
            [SerializeField] private bool _waitForComplete = true;

            public SkeletonAnimation AnimationBody => _animationBody;
            public string AnimationName => string.IsNullOrWhiteSpace(_animationName)
                ? DeathAnimationName
                : _animationName;
            public bool VisibleWhileAlive => _visibleWhileAlive;
            public bool WaitForComplete => _waitForComplete;
        }

        [SerializeField] private Transform _footAnchor;
        [SerializeField] private SkeletonAnimation _animationBody;
        [SerializeField] private SkeletonAnimation[] _additionalAnimationBodies = Array.Empty<SkeletonAnimation>();
        [SerializeField] private DeathAnimationBody[] _deathAnimationBodies = Array.Empty<DeathAnimationBody>();

        public Vector3 FootOffset => _footAnchor != null
            ? transform.position - _footAnchor.position
            : Vector3.zero;

        private Creature _bound;
        private int _previousHealth;
        private readonly List<TrackEntry> _deathTrackEntries = new();
        private int _pendingDeathAnimations;
        private Action _deathCompleteCallback;

        public void Bind(Creature creature)
        {
            _bound = creature;
            _previousHealth = creature.CurrentHP;
            _bound.OnHealthChanged += HandleHealthChanged;

            ResetAnimationBodies();

            PlayIdleAnimation(_animationBody);
            PlayAdditionalIdleAnimations();
            PlayVisibleDeathIdleAnimations();
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

            _deathCompleteCallback = onComplete;
            SetAliveAnimationBodiesActive(false, true);
            PlayDeathAnimations();

            if (_pendingDeathAnimations == 0)
                CompleteDeath();
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

            SetAliveAnimationBodiesActive(true);
            ResetDeathAnimationBodies();
        }

        private void HandleDeathAnimationComplete(TrackEntry trackEntry)
        {
            if (!_deathTrackEntries.Remove(trackEntry)) return;

            trackEntry.Complete -= HandleDeathAnimationComplete;
            _pendingDeathAnimations = Mathf.Max(0, _pendingDeathAnimations - 1);

            if (_pendingDeathAnimations > 0) return;

            CompleteDeath();
        }

        private void ClearDeathCallback()
        {
            foreach (var trackEntry in _deathTrackEntries)
            {
                if (trackEntry != null)
                    trackEntry.Complete -= HandleDeathAnimationComplete;
            }

            _deathTrackEntries.Clear();
            _pendingDeathAnimations = 0;
            _deathCompleteCallback = null;
        }

        private void SetAliveAnimationBodiesActive(bool isActive, bool keepVisibleDeathBodies = false)
        {
            SetAliveAnimationBodyActive(_animationBody, isActive, keepVisibleDeathBodies);

            if (_additionalAnimationBodies == null) return;

            foreach (var animationBody in _additionalAnimationBodies)
                SetAliveAnimationBodyActive(animationBody, isActive, keepVisibleDeathBodies);
        }

        private void SetAliveAnimationBodyActive(SkeletonAnimation animationBody, bool isActive, bool keepVisibleDeathBodies)
        {
            if (animationBody == null) return;
            if (!isActive && keepVisibleDeathBodies && IsVisibleDeathAnimationBody(animationBody)) return;

            animationBody.gameObject.SetActive(isActive);
        }

        private void PlayAdditionalIdleAnimations()
        {
            if (_additionalAnimationBodies == null) return;

            foreach (var animationBody in _additionalAnimationBodies)
                PlayIdleAnimation(animationBody);
        }

        private static void PlayIdleAnimation(SkeletonAnimation animationBody)
        {
            if (animationBody == null) return;
            if (animationBody.Skeleton.Data.FindAnimation(IdleAnimationName) == null) return;

            animationBody.AnimationState.SetAnimation(0, IdleAnimationName, true);
        }

        private void PlayVisibleDeathIdleAnimations()
        {
            if (_deathAnimationBodies == null) return;

            foreach (var deathAnimationBody in _deathAnimationBodies)
            {
                if (deathAnimationBody == null || !deathAnimationBody.VisibleWhileAlive) continue;

                PlayIdleAnimation(deathAnimationBody.AnimationBody);
            }
        }

        private void PlayDeathAnimations()
        {
            if (_deathAnimationBodies == null) return;

            foreach (var deathAnimationBody in _deathAnimationBodies)
                PlayDeathAnimation(deathAnimationBody);
        }

        private void PlayDeathAnimation(DeathAnimationBody deathAnimationBody)
        {
            if (deathAnimationBody == null) return;

            var animationBody = deathAnimationBody.AnimationBody;
            if (animationBody == null) return;

            var animationName = deathAnimationBody.AnimationName;
            animationBody.gameObject.SetActive(true);
            animationBody.AnimationState.ClearTracks();
            animationBody.Skeleton.SetToSetupPose();

            if (animationBody.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[CreatureView] Death animation '{animationName}' is missing on '{animationBody.name}' for '{name}'.");
                return;
            }

            var trackEntry = animationBody.AnimationState.SetAnimation(0, animationName, false);
            if (!deathAnimationBody.WaitForComplete) return;

            _pendingDeathAnimations++;
            _deathTrackEntries.Add(trackEntry);
            trackEntry.Complete += HandleDeathAnimationComplete;
        }

        private void ResetDeathAnimationBodies()
        {
            if (_deathAnimationBodies == null) return;

            foreach (var deathAnimationBody in _deathAnimationBodies)
            {
                if (deathAnimationBody == null || deathAnimationBody.AnimationBody == null) continue;

                var animationBody = deathAnimationBody.AnimationBody;
                animationBody.AnimationState.ClearTracks();
                animationBody.Skeleton.SetToSetupPose();
                animationBody.gameObject.SetActive(deathAnimationBody.VisibleWhileAlive);
            }
        }

        private bool IsVisibleDeathAnimationBody(SkeletonAnimation animationBody)
        {
            if (_deathAnimationBodies == null) return false;

            foreach (var deathAnimationBody in _deathAnimationBodies)
            {
                if (deathAnimationBody == null) continue;
                if (deathAnimationBody.AnimationBody == animationBody && deathAnimationBody.VisibleWhileAlive)
                    return true;
            }

            return false;
        }

        private void CompleteDeath()
        {
            var callback = _deathCompleteCallback;
            ClearDeathCallback();
            callback?.Invoke();
        }
    }
}
