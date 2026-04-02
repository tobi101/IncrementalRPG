using System.Collections.Generic;
using Entity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Gameplay
{
    public class TileGrid
    {
        private Tilemap _tilemap;
        private float _spawnYOffset;
        private Vector2Int _origin;
        private TileSlot[,] _slots;
        private List<Vector2Int> _freePrimaryTiles = new();
        
        public Vector3 GetWorldPosition(Vector2Int tileCoord)
        {
            var p0 = _tilemap.GetCellCenterWorld(new Vector3Int(tileCoord.x, tileCoord.y, 0));
            var p1 = _tilemap.GetCellCenterWorld(new Vector3Int(tileCoord.x + 1, tileCoord.y + 1, 0));
            var pos = (p0 + p1) * 0.5f;
            pos.y += _spawnYOffset;
            return pos;
        }

        public Vector2Int WorldToTile(Vector3 worldPos)
        {
            var cell = _tilemap.WorldToCell(worldPos);
            return new Vector2Int(cell.x, cell.y);
        }

        public IEnumerable<Creature> GetAllPrimaries()
        {
            foreach (var slot in _slots)
                if (slot?.Primary != null)
                    yield return slot.Primary;
        }

        public bool TryGetPrimary(Vector2Int tileCoord, out Creature creature)
        {
            var local = tileCoord - _origin;
            if (local.x < 0 || local.y < 0 || local.x >= _slots.GetLength(0) || local.y >= _slots.GetLength(1))
            {
                creature = null;
                return false;
            }
            creature = _slots[local.x, local.y].Primary;
            return creature != null;
        }

        public bool TryGetRandomFreeTile(out Vector2Int tileCoord)
        {
            if (_freePrimaryTiles.Count == 0)
            {
                tileCoord = default;
                return false;
            }
            tileCoord = _freePrimaryTiles[Random.Range(0, _freePrimaryTiles.Count)];
            return true;
        }

        public void Place(Creature creature)
        {
            var slot = GetSlot(creature.TileCoord);
            if (creature.Config.canCoexistWithOthers)
            {
                slot.Coexisting.Add(creature);
            }
            else
            {
                slot.Primary = creature;
                _freePrimaryTiles.Remove(creature.TileCoord);
            }
        }

        public void Free(Creature creature)
        {
            var slot = GetSlot(creature.TileCoord);
            if (creature.Config.canCoexistWithOthers)
            {
                slot.Coexisting.Remove(creature);
            }
            else
            {
                slot.Primary = null;
                _freePrimaryTiles.Add(creature.TileCoord);
            }
        }

        public void Initialize(Tilemap tilemap, float spawnYOffset = 0f)
        {
            _tilemap = tilemap;
            _spawnYOffset = spawnYOffset;

            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            _origin = new Vector2Int(bounds.xMin, bounds.yMin);
            var sizeX = bounds.size.x;
            var sizeY = bounds.size.y;

            _slots = new TileSlot[sizeX, sizeY];
            for (var x = 0; x < sizeX; x++)
            for (var y = 0; y < sizeY; y++)
            {
                _slots[x, y] = new TileSlot();
                _freePrimaryTiles.Add(new Vector2Int(x + _origin.x, y + _origin.y));
            }
        }

        private TileSlot GetSlot(Vector2Int tileCoord)
        {
            var local = tileCoord - _origin;
            return _slots[local.x, local.y];
        }

        private class TileSlot
        {
            public Creature Primary;
            public readonly List<Creature> Coexisting = new();
        }
    }
}
