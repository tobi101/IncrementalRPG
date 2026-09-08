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
        private readonly List<Vector2Int> _freeTiles = new();
        private readonly Dictionary<Creature, Vector2Int> _moveReservations = new();
        private int _totalTileCount;

        public int TotalTileCount => _totalTileCount;
        public int FreeTileCount => _freeTiles.Count;
        
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

        public IEnumerable<Creature> GetAll()
        {
            if (_slots == null) yield break;
            foreach (var slot in _slots)
                if (slot?.Creature != null)
                    yield return slot.Creature;
        }

        public bool TryGet(Vector2Int tileCoord, out Creature creature)
        {
            if (!TryGetSlot(tileCoord, out var slot))
            {
                creature = null;
                return false;
            }
            creature = slot.Creature;
            return creature != null;
        }

        public bool TryGetRandomFreeTile(out Vector2Int tileCoord)
        {
            if (_freeTiles.Count == 0)
            {
                tileCoord = default;
                return false;
            }
            tileCoord = _freeTiles[Random.Range(0, _freeTiles.Count)];
            return true;
        }

        public void Place(Creature creature)
        {
            if (!IsFree(creature.TileCoord))
                throw new System.InvalidOperationException($"Cell {creature.TileCoord} is unavailable.");
            var slot = GetSlot(creature.TileCoord);
            slot.Creature = creature;
            _freeTiles.Remove(creature.TileCoord);
            creature.SetTilePosition(creature.TileCoord, GetWorldPosition(creature.TileCoord));
        }

        public void Free(Creature creature)
        {
            CancelMove(creature);
            if (!TryGetSlot(creature.TileCoord, out var slot) || slot.Creature != creature)
                return;
            slot.Creature = null;
            _freeTiles.Add(creature.TileCoord);
        }

        public bool IsFree(Vector2Int coord)
        {
            return TryGetSlot(coord, out var slot) && slot.Creature == null && slot.ReservedBy == null;
        }

        public bool CanMoveTo(Creature creature, Vector2Int destination)
        {
            if (!creature.IsAlive || !TryGet(creature.TileCoord, out var occupant) || occupant != creature
                || _moveReservations.ContainsKey(creature) || !IsFree(destination))
                return false;

            var offset = destination - creature.TileCoord;
            if (offset == Vector2Int.zero || Mathf.Abs(offset.x) > 1 || Mathf.Abs(offset.y) > 1)
                return false;

            // Do not cut corners or cross another creature's diagonal path.
            return offset.x == 0 || offset.y == 0
                || (IsFree(creature.TileCoord + new Vector2Int(offset.x, 0))
                    && IsFree(creature.TileCoord + new Vector2Int(0, offset.y)));
        }

        public bool TryReserveMove(Creature creature, Vector2Int destination)
        {
            if (!CanMoveTo(creature, destination)) return false;

            GetSlot(destination).ReservedBy = creature;
            _moveReservations.Add(creature, destination);
            _freeTiles.Remove(destination);
            return true;
        }

        public bool CompleteMove(Creature creature)
        {
            if (!creature.IsAlive || !_moveReservations.TryGetValue(creature, out var destination))
                return false;
            if (!TryGet(creature.TileCoord, out var occupant) || occupant != creature)
                return false;

            var source = creature.TileCoord;
            var targetSlot = GetSlot(destination);
            if (targetSlot.ReservedBy != creature || targetSlot.Creature != null) return false;

            GetSlot(source).Creature = null;
            _freeTiles.Add(source);
            targetSlot.ReservedBy = null;
            targetSlot.Creature = creature;
            _moveReservations.Remove(creature);
            creature.SetTilePosition(destination, GetWorldPosition(destination));
            return true;
        }

        public void CancelMove(Creature creature)
        {
            if (!_moveReservations.Remove(creature, out var destination)) return;
            if (!TryGetSlot(destination, out var slot) || slot.ReservedBy != creature) return;

            slot.ReservedBy = null;
            if (slot.Creature == null)
                _freeTiles.Add(destination);
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

            _freeTiles.Clear();
            _moveReservations.Clear();
            _totalTileCount = 0;
            _slots = new TileSlot[sizeX, sizeY];
            for (var x = 0; x < sizeX; x++)
            for (var y = 0; y < sizeY; y++)
            {
                if (!tilemap.HasTile(new Vector3Int(x + _origin.x, y + _origin.y, 0))) continue;
                _slots[x, y] = new TileSlot();
                _freeTiles.Add(new Vector2Int(x + _origin.x, y + _origin.y));
                _totalTileCount++;
            }
        }

        private TileSlot GetSlot(Vector2Int tileCoord)
        {
            var local = tileCoord - _origin;
            return _slots[local.x, local.y];
        }

        private bool TryGetSlot(Vector2Int tileCoord, out TileSlot slot)
        {
            var local = tileCoord - _origin;
            slot = null;
            if (_slots == null || local.x < 0 || local.y < 0
                || local.x >= _slots.GetLength(0) || local.y >= _slots.GetLength(1)) return false;

            slot = _slots[local.x, local.y];
            return slot != null;
        }

        private class TileSlot
        {
            public Creature Creature;
            public Creature ReservedBy;
        }
    }
}
