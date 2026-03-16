using IncrementalRPG.Scripts.Reflex;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IncrementalRPG.Scripts.Core
{
    public class GameLoop : IStartable, ITickable
    {
        private readonly List<IGameSystem> _systems;

        public GameLoop(IEnumerable<IGameSystem> systems)
        {
            _systems = systems.ToList();
        }

        public void OnStart()
        {
            foreach (var t in _systems)
                t.Initialize();

            Debug.Log($"[GameLoop] Started. Systems: {_systems.Count}");
        }

        public void Tick(float deltaTime)
        {
            foreach (var t in _systems)
                t.Update(deltaTime);
        }
    }
}
