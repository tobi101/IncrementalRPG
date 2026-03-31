using System;
using System.Collections.Generic;
using Entity;
using IncrementalRPG.Scripts.AudioManager;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Gameplay
{
    public class DamageZone : IService
    {
        public enum State { Idle, Attacking }

        private readonly TileGrid _tileGrid;
        private readonly DamageZoneConfig _config;
        private readonly DamageZoneView _view;
        private readonly AudioManager _audioManager;

        private readonly List<Creature> _creaturesInZone = new List<Creature>();

        private Vector3 _worldPosition;
        private float _tickTimer;

        public Vector3 WorldPosition => _worldPosition;
        public State CurrentState { get; private set; } = State.Idle;

        public event Action<State> OnStateChanged;
        public event Action OnDamageTick;

        public DamageZone(TileGrid tileGrid, DamageZoneConfig config, DamageZoneView view, AudioManager audioManager)
        {
            _tileGrid = tileGrid;
            _config = config;
            _view = view;
            _audioManager = audioManager;
        }

        public void Initialize()
        {
            _view.Bind(this);
        }

        public void Update(float deltaTime)
        {
            UpdateWorldPosition();
            RefreshCreaturesInZone();
            UpdateState();
            TickDamage(deltaTime);
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
            var a = _config.detectionRadiusX;
            var b = _config.detectionRadiusY;

            foreach (var creature in _tileGrid.GetAllPrimaries())
            {
                var creatureWorldPos = _tileGrid.GetWorldPosition(creature.TileCoord);
                var dx = (creatureWorldPos.x - _worldPosition.x) / a;
                var dy = (creatureWorldPos.y - _worldPosition.y) / b;
                if (dx * dx + dy * dy <= 1f)
                    _creaturesInZone.Add(creature);
            }
        }

        private void UpdateState()
        {
            var newState = _creaturesInZone.Count > 0 ? State.Attacking : State.Idle;
            if (newState == CurrentState) return;

            if (newState == State.Attacking)
                _tickTimer = 0f;

            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }

        private void TickDamage(float deltaTime)
        {
            if (CurrentState != State.Attacking) return;

            _tickTimer += deltaTime;
            if (_tickTimer < _config.tickInterval) return;

            _tickTimer = 0f;
            for (var i = 0; i < _creaturesInZone.Count; i++)
            {
                _creaturesInZone[i].TakeDamage(_config.damagePerTick);
                _audioManager.PlayHitAudio(i * 0.1f);
            }

            _audioManager.PlayWaveAudio();
            OnDamageTick?.Invoke();
        }
    }
}
