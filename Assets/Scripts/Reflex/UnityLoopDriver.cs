using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

namespace IncrementalRPG.Scripts.Reflex
{
    public class UnityLoopDriver : MonoBehaviour
    {
        [Inject] private IEnumerable<IAwakeable> awakeables;
        [Inject] private IEnumerable<IStartable> startables;
        [Inject] private IEnumerable<ITickable> tickables;
        [Inject] private IEnumerable<IFixedTickable> fixedTickables;
        [Inject] private IEnumerable<ILateTickable> lateTickables;

        private void Awake()
        {
            foreach (var a in awakeables)
                a.OnAwake();
        }

        private void Start()
        {
            foreach (var s in startables)
                s.OnStart();
        }

        private void Update()
        {
            foreach (var t in tickables)
                t.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            foreach (var f in fixedTickables)
                f.FixedTick(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            foreach (var l in lateTickables)
                l.LateTick(Time.deltaTime);
        }
    }
}