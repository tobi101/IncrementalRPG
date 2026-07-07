using System;
using System.Collections.Generic;
using Core.TestSkillTree;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Gameplay
{
    public class DamageZone : IService
    {
        public enum State { Idle, Attacking }
        public enum AttackSource { Manual, Auto }

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

        public Vector3 WorldPosition => _worldPosition;
        public float RadiusX => _config.baseRadius * _skillTree.GetMultiplier(StatType.ZoneRadius);
        public float RadiusY => RadiusX * _config.aspectRatio;
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
        }

        public void UpdateAim()
        {
            if (_inputBlocker != null && _inputBlocker.IsBlocked)
                return;

            UpdateWorldPosition();
        }

        private void UpdateWorldPosition()
        {
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
            var radiusX = RadiusX + hitRadius;
            var radiusY = RadiusY + hitRadius;

            var footWorldPos = _tileGrid.GetWorldPosition(creature.TileCoord);
            var dx = (footWorldPos.x - _worldPosition.x) / radiusX;
            var dy = (footWorldPos.y - _worldPosition.y) / radiusY;

            return dx * dx + dy * dy <= 1f;
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

        private bool WasManualAttackRequested()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private float GetManualAttackCooldown()
        {
            return GetAttackInterval(_config.baseManualAttackCooldown, StatType.ManualAttackSpeed);
        }

        private float GetAutoAttackInterval()
        {
            return GetAttackInterval(_config.baseAutoAttackInterval, StatType.AutoAttackSpeed);
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

            var damage = (int)((_config.damagePerTick + _skillTree.GetBonus(StatType.ZoneDamage)) * _skillTree.GetMultiplier(StatType.ZoneDamage));
            for (var i = 0; i < _creaturesInZone.Count; i++)
            {
                _creaturesInZone[i].TakeDamage(damage);
                _audioManager.PlayHitAudio(i * 0.1f);
            }

            _audioManager.PlayWaveAudio();
            OnZoneTick?.Invoke(source);
        }

        private void StopDamageRegistration()
        {
            _creaturesInZone.Clear();
            _manualAttackCooldownRemaining = 0f;
            _autoAttackTimer = 0f;

            if (CurrentState == State.Idle)
                return;

            CurrentState = State.Idle;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
