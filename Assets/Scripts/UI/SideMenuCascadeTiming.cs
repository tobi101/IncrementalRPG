using UnityEngine;

namespace UI
{
    public static class SideMenuCascadeTiming
    {
        public static float Evaluate(float elapsed, int itemIndex, float itemDuration, float itemDelay)
        {
            var delay = Mathf.Max(0, itemIndex) * Mathf.Max(0f, itemDelay);
            if (itemDuration <= 0f)
                return elapsed >= delay ? 1f : 0f;

            return Mathf.Clamp01((elapsed - delay) / itemDuration);
        }
    }
}
