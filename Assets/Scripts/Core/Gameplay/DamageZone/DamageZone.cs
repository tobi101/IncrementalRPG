using System;
using System.Collections.Generic;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using Model;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Core.Gameplay
{
    public class DamageZone : IService
    {
        public enum State { Idle, Attacking }
        public enum AttackSource { Manual, Auto, Special }

        private const float MinAttackInterval = 0.05f;
        private const float MinAttackSpeedMultiplier = 0.01f;

        private readonly TileGrid _tileGrid;
        private readonly DamageZoneConfig _config;
        private readonly DamageZoneView _view;
        private readonly AudioManager _audioManager;
        private readonly Player _player;
        private readonly SkillTreeService _skillTree;
        private readonly GameplayInputBlocker _inputBlocker;

        private readonly List<Creature> _creaturesInZone = new List<Creature>();

        private Vector3 _worldPosition;
        private float _manualAttackCooldownRemaining;
        private float _autoAttackTimer;
        private float _specialAttackCooldownRemaining;

        public Vector3 WorldPosition => _worldPosition;
        public float RadiusX => _config.baseRadius * _skillTree.GetMultiplier(StatType.ZoneRadius);
        public float RadiusY => RadiusX * _config.aspectRatio;
        public bool IsSpecialAttackUnlocked => _skillTree.IsUnlocked(GameFeature.SpecialAttack);
        public bool IsSpecialAttackReady => IsSpecialAttackUnlocked && _specialAttackCooldownRemaining <= 0f;
        public float SpecialAttackCooldownProgress
        {
            get
            {
                if (!IsSpecialAttackUnlocked)
                    return 0f;

                var duration = GetSpecialAttackCooldown();
                if (duration <= 0f)
                    return 1f;

                return 1f - Mathf.Clamp01(_specialAttackCooldownRemaining / duration);
            }
        }

        public State CurrentState { get; private set; } = State.Idle;

        public event Action<State> OnStateChanged;
        public event Action<AttackSource> OnZoneTick;

        public DamageZone(TileGrid tileGrid, DamageZoneConfig config, DamageZoneView view, AudioManager audioManager, Player player,
            SkillTreeService skillTree, GameplayInputBlocker inputBlocker)
        {
            _tileGrid = tileGrid;
            _config = config;
            _view = view;
            _audioManager = audioManager;
            _player = player;
            _skillTree = skillTree;
            _inputBlocker = inputBlocker;
        }

        public void Initialize()
        {
            _view.Bind(this);
        }

        public void Update(float deltaTime)
        {
            if (_inputBlocker != null && _inputBlocker.IsBlocked)
            {
                StopDamageRegistration();
                return;
            }

            UpdateAim();
            UpdateManualAttack(deltaTime);
            UpdateAutoAttack(deltaTime);
            UpdateSpecialAttack(deltaTime);
        }

        public void UpdateAim()
        {
            if (_inputBlocker != null && _inputBlocker.IsBlocked)
                return;

            UpdateWorldPosition();
        }

        public bool ContainsWorldCircle(Vector3 worldPosition, float hitRadius = 0f)
        {
            hitRadius = Mathf.Max(0f, hitRadius);
            var radiusX = RadiusX + hitRadius;
            var radiusY = RadiusY + hitRadius;
            if (radiusX <= 0f || radiusY <= 0f)
                return false;

            var dx = (worldPosition.x - _worldPosition.x) / radiusX;
            var dy = (worldPosition.y - _worldPosition.y) / radiusY;
            return dx * dx + dy * dy <= 1f;
        }

        private void UpdateWorldPosition()
        {
            if (Mouse.current == null || Camera.main == null)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            _worldPosition = new Vector3(worldPos.x, worldPos.y, 0f);
        }

        private void RefreshCreaturesInZone()
        {
            _creaturesInZone.Clear();

            foreach (var creature in _tileGrid.GetAll())
            {
                if (!creature.IsAlive) continue;

                if (IntersectsCreatureHitArea(creature))
                    _creaturesInZone.Add(creature);
            }
        }

        private bool IntersectsCreatureHitArea(Creature creature)
        {
            var hitRadius = Mathf.Max(0f, creature.Config.damageZoneHitRadius);
            var footWorldPos = _tileGrid.GetWorldPosition(creature.TileCoord);
            return ContainsWorldCircle(footWorldPos, hitRadius);
        }

        private void UpdateState()
        {
            var newState = _creaturesInZone.Count > 0 ? State.Attacking : State.Idle;
            if (newState == CurrentState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }

        private void UpdateManualAttack(float deltaTime)
        {
            if (_manualAttackCooldownRemaining > 0f)
                _manualAttackCooldownRemaining = Mathf.Max(0f, _manualAttackCooldownRemaining - deltaTime);

            if (!WasManualAttackRequested())
                return;

            if (_manualAttackCooldownRemaining > 0f)
                return;

            PerformAttack(AttackSource.Manual);
            _manualAttackCooldownRemaining = GetManualAttackCooldown();
        }

        private void UpdateAutoAttack(float deltaTime)
        {
            if (!_skillTree.IsUnlocked(GameFeature.AutoAttack))
            {
                _autoAttackTimer = 0f;
                return;
            }

            _autoAttackTimer += deltaTime;
            var autoAttackInterval = GetAutoAttackInterval();
            if (_autoAttackTimer < autoAttackInterval)
                return;

            _autoAttackTimer = 0f;
            PerformAttack(AttackSource.Auto);
        }

        private void UpdateSpecialAttack(float deltaTime)
        {
            if (!IsSpecialAttackUnlocked)
            {
                _specialAttackCooldownRemaining = 0f;
                return;
            }

            if (_specialAttackCooldownRemaining > 0f)
                _specialAttackCooldownRemaining = Mathf.Max(0f, _specialAttackCooldownRemaining - deltaTime);

            if (!WasSpecialAttackRequested() || !IsSpecialAttackReady)
                return;

            // Set the cooldown before raising OnZoneTick so the view observes progress = 0 on the attack frame.
            _specialAttackCooldownRemaining = GetSpecialAttackCooldown();
            PerformAttack(AttackSource.Special);
        }

        private bool WasManualAttackRequested()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private bool WasSpecialAttackRequested()
        {
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        }

        private float GetManualAttackCooldown()
        {
            return GetAttackInterval(_config.baseManualAttackCooldown, StatType.ManualAttackSpeed);
        }

        private float GetAutoAttackInterval()
        {
            return GetAttackInterval(_config.baseAutoAttackInterval, StatType.AutoAttackSpeed);
        }

        private float GetSpecialAttackCooldown()
        {
            return Mathf.Max(0f, _config.baseSpecialAttackCooldown);
        }

        private float GetAttackInterval(float baseInterval, StatType speedStat)
        {
            var attackSpeed = GetAttackSpeedMultiplier(speedStat);
            return Mathf.Max(MinAttackInterval, Mathf.Max(MinAttackInterval, baseInterval) / attackSpeed);
        }

        private float GetAttackSpeedMultiplier(StatType speedStat)
        {
            var additiveSpeed = 1f + _skillTree.GetBonus(speedStat);
            var multiplicativeSpeed = _skillTree.GetMultiplier(speedStat);
            return Mathf.Max(MinAttackSpeedMultiplier, additiveSpeed * multiplicativeSpeed);
        }

        private void PerformAttack(AttackSource source)
        {
            RefreshCreaturesInZone();
            UpdateState();

            var damage = GetDamage(source);
            for (var i = 0; i < _creaturesInZone.Count; i++)
            {
                _creaturesInZone[i].TakeDamage(damage);
                _audioManager.PlayHitAudio(i * 0.1f);
            }

            _audioManager.PlayWaveAudio();
            OnZoneTick?.Invoke(source);
        }

        private BigDouble GetDamage(AttackSource source)
        {
            var damage = BigDouble.Max(BigDouble.Zero,
                _config.damagePerTick + _skillTree.GetBonus(StatType.ZoneDamage));
            var multiplier = Mathf.Max(0f, _skillTree.GetMultiplier(StatType.ZoneDamage));

            if (source == AttackSource.Special)
                multiplier *= Mathf.Max(0f, _config.specialAttackDamageMultiplier);

            return BigDoubleMath.MultiplyAndRound(damage, multiplier);
        }

        private void StopDamageRegistration()
        {
            _creaturesInZone.Clear();
            _manualAttackCooldownRemaining = 0f;
            _autoAttackTimer = 0f;
            _specialAttackCooldownRemaining = 0f;

            if (CurrentState == State.Idle)
                return;

            CurrentState = State.Idle;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
