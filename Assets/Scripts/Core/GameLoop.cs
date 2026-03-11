using IncrementalRPG.Scripts.Reflex;
using UnityEngine;

namespace IncrementalRPG.Scripts.Core
{
    public class GameLoop : IStartable, ITickable
    {
        public void OnStart()
        {
            Debug.Log("[GameLoop] Started");
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
