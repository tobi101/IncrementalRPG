using System.Collections.Generic;
using System.Linq;
using IncrementalRPG.Scripts.Core;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;
using UnityEngine;

namespace Core
{
    public class GameLoop : IStartable, ITickable
    {
        private readonly List<IService> _services;

        public GameLoop(IEnumerable<IService> systems)
        {
            _services = systems.ToList();
        }

        public void OnStart()
        {
            foreach (var t in _services)
                t.Initialize();

            Debug.Log($"[GameLoop] Started. Systems: {_services.Count}");
        }

        public void Tick(float deltaTime)
        {
            foreach (var t in _services)
                t.Update(deltaTime);
        }
    }
}
