using UnityEngine;

namespace Core.Gameplay.Bomb
{
    [CreateAssetMenu(fileName = "BombExplosionConfig", menuName = "IncrementalRPG/Bomb Explosion Config")]
    public class BombExplosionConfig : ScriptableObject
    {
        public int baseDamage = 30;
        public float baseRadius = 2f;
        public float aspectRatio = 0.55f;
    }
}
