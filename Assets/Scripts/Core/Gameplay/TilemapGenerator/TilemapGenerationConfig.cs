using UnityEngine;

namespace Core.Gameplay
{
    public enum TilemapGradientDirection
    {
        BottomToTop,
        TopToBottom,
        LeftToRight,
        RightToLeft,
        BottomLeftToTopRight,
        BottomRightToTopLeft,
    }

    [CreateAssetMenu(fileName = "TilemapGenerationConfig", menuName = "RPG/Tilemap Generation Config")]
    public class TilemapGenerationConfig : ScriptableObject
    {
        [Header("Tile Set")]
        public TilemapTileSet tileSet;

        [Header("Gradient")]
        public TilemapGradientDirection gradientDirection = TilemapGradientDirection.BottomToTop;
        public AnimationCurve gradientRemap = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Random")]
        public int seed = 12345;
        [Tooltip("Масштаб волнистости границы: меньше = плавнее, больше = мельче")]
        [Min(0.001f)] public float noiseScale = 0.15f;
        [Tooltip("Сила смещения границы шумом: 0 = ровная линия, 1 = сильные волны")]
        [Range(0f, 1f)] public float noiseBorderStrength = 0.25f;

        [Header("Pillar")]
        [Min(0)] public int pillarHeight = 3;
    }
}
