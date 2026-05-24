using System;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public sealed class DungeonLevelTransitionConfig
    {
        [Min(0f)] public float closeDuration = 0.75f;
        [Min(0f)] public float holdDuration;
        [Min(0f)] public float openDuration = 0.75f;

        public float CloseDuration => Mathf.Max(0f, closeDuration);

        public float HoldDuration => Mathf.Max(0f, holdDuration);

        public float OpenDuration => Mathf.Max(0f, openDuration);
    }
}
