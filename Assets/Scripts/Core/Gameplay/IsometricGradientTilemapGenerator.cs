using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Gameplay
{
    [DisallowMultipleComponent]
    public class IsometricGradientTilemapGenerator : MonoBehaviour
    {
        [Serializable]
        public class TileVariant
        {
            public TileBase tile;
            [Range(0f, 1f)] public float probability = 0.1f;
        }

        [Serializable]
        public class TileRule
        {
            public TileBase tile;
            public List<TileVariant> variants = new();
            [Range(0f, 1f)] public float gradientMin = 0f;
            [Range(0f, 1f)] public float gradientMax = 1f;
        }

        [Header("References")]
        [SerializeField] private Tilemap targetTilemap;

        [Header("Grid")]
        [Min(1)]
        [SerializeField] private int size = 24;
        [SerializeField] private Vector3Int origin = Vector3Int.zero;

        public enum GradientDirection
        {
            BottomToTop,        // снизу вверх (по Y)
            TopToBottom,        // сверху вниз
            LeftToRight,        // слева направо (по X)
            RightToLeft,        // справа налево
            BottomLeftToTopRight,  // угол → угол (X+Y)
            BottomRightToTopLeft,  // угол → угол (-X+Y)
        }

        [Header("Gradient")]
        [SerializeField] private GradientDirection gradientDirection = GradientDirection.BottomToTop;
        [SerializeField] private AnimationCurve gradientRemap = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Random")]
        [SerializeField] private int seed = 12345;
        [Tooltip("Масштаб волнистости границы: меньше = плавнее, больше = мельче")]
        [Min(0.001f)] [SerializeField] private float noiseScale = 0.15f;
        [Tooltip("Сила смещения границы шумом: 0 = ровная линия, 1 = сильные волны")]
        [Range(0f, 1f)] [SerializeField] private float noiseBorderStrength = 0.25f;

        [Header("Tile rules")]
        [SerializeField] private List<TileRule> tileRules = new();

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (targetTilemap == null)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Target Tilemap is not assigned.");
                return;
            }

            if (tileRules.Count == 0)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Add at least one TileRule.");
                return;
            }

            targetTilemap.ClearAllTiles();

            var denominator = Mathf.Max(1, size - 1);

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var gradient = gradientDirection switch
                    {
                        GradientDirection.BottomToTop         => (float)y / denominator,
                        GradientDirection.TopToBottom         => 1f - (float)y / denominator,
                        GradientDirection.LeftToRight         => (float)x / denominator,
                        GradientDirection.RightToLeft         => 1f - (float)x / denominator,
                        GradientDirection.BottomLeftToTopRight => (x + y) / (2f * denominator),
                        GradientDirection.BottomRightToTopLeft => (size - 1 - x + y) / (2f * denominator),
                        _                                     => (float)y / denominator,
                    };
                    gradient = Mathf.Clamp01(gradient);
                    gradient = gradientRemap.Evaluate(gradient);

                    var noiseOffset = (seed % 9973) * 0.1f;
                    var noise = Mathf.PerlinNoise(noiseOffset + x * noiseScale, noiseOffset + y * noiseScale);
                    gradient = Mathf.Clamp01(gradient + (noise - 0.5f) * noiseBorderStrength);

                    var rule = PickRule(gradient);
                    var tile = PickTile(rule, x, y);
                    var position = origin + new Vector3Int(x, y, 0);

                    targetTilemap.SetTile(position, tile);
                }
            }

            Debug.Log($"[IsometricGradientTilemapGenerator] Generated {size}x{size} Bottom->Top gradient (seed: {seed}).");
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            if (targetTilemap == null)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Target Tilemap is not assigned.");
                return;
            }

            targetTilemap.ClearAllTiles();
            Debug.Log("[IsometricGradientTilemapGenerator] Tilemap cleared.");
        }

        private TileRule PickRule(float gradient)
        {
            var bestWeight = -1f;
            var bestIndex = 0;

            for (var i = 0; i < tileRules.Count; i++)
            {
                var rule = tileRules[i];
                if (rule.tile == null)
                    continue;

                var halfWidth = (rule.gradientMax - rule.gradientMin) * 0.5f;
                if (halfWidth <= 0f)
                    continue;

                var center = rule.gradientMin + halfWidth;
                var weight = 1f - Mathf.Abs(gradient - center) / halfWidth;
                weight = Mathf.Max(0f, weight);

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestIndex = i;
                }
            }

            return tileRules[bestIndex];
        }

        private TileBase PickTile(TileRule rule, int x, int y)
        {
            if (rule.variants == null || rule.variants.Count == 0)
                return rule.tile;

            var random01 = HashTo01(seed, x, y, 83);
            var cumulative = 0f;

            foreach (var variant in rule.variants)
            {
                if (variant.tile == null)
                    continue;

                cumulative += variant.probability;
                if (random01 < cumulative)
                    return variant.tile;
            }

            return rule.tile;
        }

        private static float HashTo01(int a, int b, int c, int salt)
        {
            unchecked
            {
                var h = (uint)a;
                h = (h * 397u) ^ (uint)b;
                h = (h * 397u) ^ (uint)c;
                h = (h * 397u) ^ (uint)salt;
                h ^= h >> 16;
                h *= 2246822519u;
                h ^= h >> 13;
                h *= 3266489917u;
                h ^= h >> 16;

                return (h & 0x00FFFFFFu) / 16777215f;
            }
        }
    }
}
