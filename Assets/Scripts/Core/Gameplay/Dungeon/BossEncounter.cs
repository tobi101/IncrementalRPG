using System;
using Entity;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    internal sealed class BossEncounter : IDisposable
    {
        private readonly BossEncounterConfig _config;
        private readonly SpawnService _spawnService;
        private readonly Vector3[] _totemPositions;
        private readonly Creature[] _totems;
        private readonly BossEncounterView _view;
        private float _restoreTimeLeft;

        public Creature Boss { get; }
        public float RestoreTimeLeft => _restoreTimeLeft;

        public BossEncounter(BossEncounterConfig config, SpawnService spawnService, Bounds arena, out Bounds cameraBounds)
        {
            _config = config;
            _spawnService = spawnService;
            _totemPositions = new Vector3[config.totemPositions.Length];
            _totems = new Creature[_totemPositions.Length];
            cameraBounds = arena;

            var center = arena.center;
            center.z = 0f;
            // Area attacks visit creatures in spawn order. The attack that breaks the last totem still hits the shield.
            Boss = spawnService.SpawnAt(config.boss, center);
            for (var i = 0; i < _totems.Length; i++)
            {
                var offset = config.totemPositions[i];
                var position = center + new Vector3(offset.x * arena.extents.x, offset.y * arena.extents.y);
                _totemPositions[i] = position;
                cameraBounds.Encapsulate(new Bounds(position, Vector3.one));
                SpawnTotem(i);
            }

            _view = new BossEncounterView(config, center, _totemPositions);
            cameraBounds.Encapsulate(_view.ShieldBounds);
            RefreshProtection();
            Boss.OnDamageBlocked += _view.FlashShield;
        }

        public void ShowEffects() => _view.Show();

        public void Tick(float deltaTime)
        {
            if (_restoreTimeLeft > 0f)
            {
                _restoreTimeLeft = Mathf.Max(0f, _restoreTimeLeft - deltaTime);
                if (_restoreTimeLeft == 0f)
                {
                    for (var i = 0; i < _totems.Length; i++)
                        if (!_totems[i].IsAlive)
                            SpawnTotem(i);

                    RefreshProtection();
                }
            }

            _view.Tick(deltaTime);
        }

        public void Dispose()
        {
            Boss.OnDamageBlocked -= _view.FlashShield;
            foreach (var totem in _totems)
                totem.OnDied -= HandleTotemDestroyed;

            _view.Dispose();
        }

        private void SpawnTotem(int index)
        {
            var totem = _spawnService.SpawnAt(_config.totem, _totemPositions[index]);
            _totems[index] = totem;
            totem.OnDied += HandleTotemDestroyed;
        }

        private void HandleTotemDestroyed()
        {
            if (_restoreTimeLeft <= 0f)
                _restoreTimeLeft = _config.restoreDelay;

            RefreshProtection();
        }

        private void RefreshProtection()
        {
            var shielded = false;
            for (var i = 0; i < _totems.Length; i++)
            {
                var alive = _totems[i].IsAlive;
                _view.SetLinkActive(i, alive);
                shielded |= alive;
            }

            Boss.IsDamageBlocked = shielded;
            _view.SetShieldVisible(shielded);
        }
    }
}
