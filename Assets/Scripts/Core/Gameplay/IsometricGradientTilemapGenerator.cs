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
        public class TileRule
        {
            public string name = "Tile";
            public TileBase[] tiles = Array.Empty<TileBase>();
            public AnimationCurve weightByGradient = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        }

        [Header("References")]
        [SerializeField] private Tilemap targetTilemap;

        [Header("Grid")]
        [Min(1)]
        [SerializeField] private int size = 24;
        [SerializeField] private Vector3Int origin = Vector3Int.zero;

        [Header("Gradient")]
        [SerializeField] private Vector2 gradientCenterNormalized = new(0.5f, 0.5f);
        [SerializeField] private AnimationCurve gradientRemap = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Noise")]
        [Min(0f)]
        [SerializeField] private float noiseStrength = 0.12f;
        [Min(0.0001f)]
        [SerializeField] private float noiseScale = 0.2f;

        [Header("Random")]
        [SerializeField] private int seed = 12345;

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

            var centerX = (size - 1) * Mathf.Clamp01(gradientCenterNormalized.x);
            var centerY = (size - 1) * Mathf.Clamp01(gradientCenterNormalized.y);

            var dMax = Mathf.Max(
                ManhattanDistance(0f, 0f, centerX, centerY),
                ManhattanDistance(size - 1f, 0f, centerX, centerY),
                ManhattanDistance(0f, size - 1f, centerX, centerY),
                ManhattanDistance(size - 1f, size - 1f, centerX, centerY)
            );

            if (dMax <= 0f)
                dMax = 1f;

            var noiseOffsetX = seed * 0.173f;
            var noiseOffsetY = seed * 0.941f;

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var distance = ManhattanDistance(x, y, centerX, centerY);
                    var gradient = Mathf.Clamp01(distance / dMax);
                    gradient = gradientRemap.Evaluate(gradient);

                    var noise = Mathf.PerlinNoise((x + noiseOffsetX) * noiseScale, (y + noiseOffsetY) * noiseScale);
                    var noisyGradient = Mathf.Clamp01(gradient + (noise - 0.5f) * noiseStrength * 2f);

                    var rule = PickRule(noisyGradient, x, y);
                    var tile = PickTile(rule, x, y);
                    var position = origin + new Vector3Int(x, y, 0);

                    targetTilemap.SetTile(position, tile);
                }
            }

            Debug.Log($"[IsometricGradientTilemapGenerator] Generated {size}x{size} tiles (seed: {seed}).");
        }

        private TileRule PickRule(float gradient, int x, int y)
        {
            var totalWeight = 0f;
            var weights = new float[tileRules.Count];

            for (var i = 0; i < tileRules.Count; i++)
            {
                var rule = tileRules[i];
                var hasTile = rule.tiles != null && rule.tiles.Length > 0;

                if (!hasTile)
                {
                    weights[i] = 0f;
                    continue;
                }

                var weight = Mathf.Max(0f, rule.weightByGradient.Evaluate(gradient));
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0f)
                return FirstRuleWithTiles();

            var random01 = HashTo01(seed, x, y, 19);
            var threshold = random01 * totalWeight;
            var cumulative = 0f;

            for (var i = 0; i < tileRules.Count; i++)
            {
                cumulative += weights[i];
                if (threshold <= cumulative)
                    return tileRules[i];
            }

            return tileRules[tileRules.Count - 1];
        }

        private TileBase PickTile(TileRule rule, int x, int y)
        {
            if (rule.tiles == null || rule.tiles.Length == 0)
                return null;

            var idx = Mathf.FloorToInt(HashTo01(seed, x, y, 83) * rule.tiles.Length);
            idx = Mathf.Clamp(idx, 0, rule.tiles.Length - 1);
            return rule.tiles[idx];
        }

        private TileRule FirstRuleWithTiles()
        {
            foreach (var rule in tileRules)
            {
                if (rule.tiles != null && rule.tiles.Length > 0)
                    return rule;
            }

            return tileRules[0];
        }

        private static float ManhattanDistance(float x0, float y0, float x1, float y1)
        {
            return Mathf.Abs(x0 - x1) + Mathf.Abs(y0 - y1);
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
