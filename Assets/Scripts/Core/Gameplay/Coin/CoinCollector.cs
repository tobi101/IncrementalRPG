using System.Collections.Generic;
using IncrementalRPG.Scripts.Core;
using UnityEngine;

namespace Core.Gameplay
{
    public class CoinCollector : IService
    {
        private readonly DamageZone _damageZone;
        private readonly DamageZoneConfig _config;
        private readonly TileGrid _tileGrid;
        private readonly GoldWallet _wallet;

        private readonly List<CoinPile> _pilesInZone = new();
        private readonly Dictionary<CoinPile, float> _progress = new();

        public CoinCollector(DamageZone damageZone, DamageZoneConfig config, TileGrid tileGrid, GoldWallet wallet)
        {
            _damageZone = damageZone;
            _config = config;
            _tileGrid = tileGrid;
            _wallet = wallet;
        }

        public void Initialize() { }

        public void Update(float deltaTime)
        {
            RefreshPilesInZone();
            // UpdateProgress(deltaTime);
        }

        private void RefreshPilesInZone()
        {
            _pilesInZone.Clear();
            var center = _damageZone.WorldPosition;
            var a = _config.detectionRadiusX;
            var b = _config.detectionRadiusY;

            foreach (var pile in _tileGrid.GetAllCoinPiles())
            {
                var worldPos = _tileGrid.GetWorldPosition(pile.TileCoord);
                var dx = (worldPos.x - center.x) / a;
                var dy = (worldPos.y - center.y) / b;
                if (dx * dx + dy * dy <= 1f)
                    _pilesInZone.Add(pile);
            }
        }

        private void UpdateProgress(float deltaTime)
        {
            // Сбрасываем прогресс для пайлов, покинувших зону
            var toRemove = new List<CoinPile>();
            foreach (var key in _progress.Keys)
                if (!_pilesInZone.Contains(key))
                    toRemove.Add(key);
            foreach (var key in toRemove)
                _progress.Remove(key);

            // Накапливаем для пайлов в зоне
            var toCollect = new List<CoinPile>();
            foreach (var pile in _pilesInZone)
            {
                _progress.TryGetValue(pile, out var elapsed);
                elapsed += deltaTime;

                if (elapsed >= _config.collectTime)
                    toCollect.Add(pile);
                else
                    _progress[pile] = elapsed;
            }

            foreach (var pile in toCollect)
            {
                _progress.Remove(pile);
                /*_wallet.Add(pile.Amount);*/
                pile.Collect();
            }
        }
    }
}
