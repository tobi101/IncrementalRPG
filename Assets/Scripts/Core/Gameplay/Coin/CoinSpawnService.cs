using System.Collections.Generic;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Utils;

namespace Core.Gameplay
{
    public class CoinSpawnService : IService
    {
        private readonly SpawnService _spawnService;
        private readonly TileGrid _tileGrid;
        private readonly CoinPileView _coinViewPrefab;

        private ObjectPool<CoinPileView> _pool;
        private readonly Dictionary<CoinPile, CoinPileView> _pileViews = new();

        public CoinSpawnService(SpawnService spawnService, TileGrid tileGrid, CoinPileView coinViewPrefab)
        {
            _spawnService = spawnService;
            _tileGrid = tileGrid;
            _coinViewPrefab = coinViewPrefab;
        }

        public void Initialize()
        {
            var root = new GameObject("[CoinPool]").transform;
            _pool = new ObjectPool<CoinPileView>(_coinViewPrefab, root);
            _spawnService.OnCreatureKilled += HandleCreatureKilled;
        }

        public void Update(float deltaTime) { }

        private void HandleCreatureKilled(Vector2Int coord, int amount)
        {
        }

        private void ReturnPile(CoinPile pile)
        {
            if (!_pileViews.TryGetValue(pile, out var view)) return;

            view.Unbind();
            _pool.Return(view);
            _pileViews.Remove(pile);
        }
    }
}
