using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Core.Gameplay
{
    // Ticked by SpawnService before spawning and before attack services run.
    public sealed class EnemyMovementService
    {
        private sealed class Movement
        {
            public Creature Creature;
            public float WaitRemaining;
            public float Progress;
            public Vector3 Start;
            public Vector3 End;
        }

        private readonly TileGrid _tileGrid;
        private readonly List<Movement> _active = new();
        private readonly List<Vector2Int> _neighbors = new(8);

        public EnemyMovementService(TileGrid tileGrid)
        {
            _tileGrid = tileGrid;
        }

        public void Register(Creature creature)
        {
            if (!creature.Config.canMove || creature.Config.featureType != FeatureType.None) return;
            if (_active.Exists(entry => entry.Creature == creature)) return;
            _active.Add(new Movement { Creature = creature, WaitRemaining = GetInterval(creature) });
        }

        public void Unregister(Creature creature)
        {
            _active.RemoveAll(entry => entry.Creature == creature);
            _tileGrid.CancelMove(creature);
            creature.IsMoving = false;
        }

        public void Update(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var movement = _active[i];
                var creature = movement.Creature;
                if (!creature.IsAlive)
                {
                    Unregister(creature);
                    _tileGrid.Free(creature);
                    continue;
                }

                // Consume time across wait/step boundaries so a step lasts exactly 1 / speed seconds.
                var remaining = deltaTime;
                while (remaining > 0f)
                {
                    if (creature.IsMoving)
                    {
                        var speed = Mathf.Max(0.01f, creature.Config.moveSpeed);
                        var timeToFinish = (1f - movement.Progress) / speed;
                        var elapsed = Mathf.Min(remaining, timeToFinish);
                        movement.Progress = Mathf.Min(1f, movement.Progress + elapsed * speed);
                        remaining -= elapsed;
                        creature.WorldPosition = Vector3.Lerp(movement.Start, movement.End, movement.Progress);
                        if (elapsed < timeToFinish) break;

                        if (!_tileGrid.CompleteMove(creature))
                        {
                            _tileGrid.CancelMove(creature);
                            creature.WorldPosition = _tileGrid.GetWorldPosition(creature.TileCoord);
                        }
                        creature.IsMoving = false;
                        movement.WaitRemaining = GetInterval(creature);
                        continue;
                    }

                    var wait = Mathf.Min(remaining, movement.WaitRemaining);
                    movement.WaitRemaining -= wait;
                    remaining -= wait;
                    if (movement.WaitRemaining > 0f) break;

                    movement.WaitRemaining = GetInterval(creature);
                    if (!creature.Config.canMove || Random.value >= Mathf.Clamp01(creature.Config.moveChance))
                        continue;

                    _neighbors.Clear();
                    for (var x = -1; x <= 1; x++)
                    for (var y = -1; y <= 1; y++)
                    {
                        var destination = creature.TileCoord + new Vector2Int(x, y);
                        if (_tileGrid.CanMoveTo(creature, destination))
                            _neighbors.Add(destination);
                    }

                    if (_neighbors.Count == 0) continue;
                    var target = _neighbors[Random.Range(0, _neighbors.Count)];
                    if (!_tileGrid.TryReserveMove(creature, target)) continue;

                    movement.Start = creature.WorldPosition;
                    movement.End = _tileGrid.GetWorldPosition(target);
                    movement.Progress = 0f;
                    creature.IsMoving = true;
                }
            }
        }

        private static float GetInterval(Creature creature)
        {
            return Mathf.Max(0.01f, creature.Config.moveCheckInterval);
        }
    }
}
