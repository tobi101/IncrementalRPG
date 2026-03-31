using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Gameplay
{
    [DisallowMultipleComponent]
    public class IsometricGradientTilemapGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap targetTilemap;
        [SerializeField] private Tilemap leftPillarTilemap;
        [SerializeField] private Tilemap rightPillarTilemap;
        [SerializeField] private TilemapCameraAutoFitter cameraAutoFitter;

        [Header("Config")]
        [SerializeField] private TilemapGenerationConfig config;

        [Header("Grid")]
        [Min(1)]
        [SerializeField] private int size = 24;
        [SerializeField] private Vector3Int origin = Vector3Int.zero;

        [Header("Post Generate")]
        [SerializeField] private bool autoFitCameraAfterGenerate = true;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (targetTilemap == null)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Target Tilemap is not assigned.");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Generation Config is not assigned.");
                return;
            }

            if (config.tileSet == null)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Tile Set is not assigned in config.");
                return;
            }

            if (config.tileSet.tileRules.Count == 0)
            {
                Debug.LogError("[IsometricGradientTilemapGenerator] Add at least one TileRule to the Tile Set.");
                return;
            }

            targetTilemap.ClearAllTiles();

            var denominator = Mathf.Max(1, size - 1);

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var gradient = config.gradientDirection switch
                    {
                        TilemapGradientDirection.BottomToTop          => (float)y / denominator,
                        TilemapGradientDirection.TopToBottom          => 1f - (float)y / denominator,
                        TilemapGradientDirection.LeftToRight          => (float)x / denominator,
                        TilemapGradientDirection.RightToLeft          => 1f - (float)x / denominator,
                        TilemapGradientDirection.BottomLeftToTopRight => (x + y) / (2f * denominator),
                        TilemapGradientDirection.BottomRightToTopLeft => (size - 1 - x + y) / (2f * denominator),
                        _                                             => (float)y / denominator,
                    };
                    gradient = Mathf.Clamp01(gradient);
                    gradient = config.gradientRemap.Evaluate(gradient);

                    var noiseOffset = (config.seed % 9973) * 0.1f;
                    var noise = Mathf.PerlinNoise(noiseOffset + x * config.noiseScale, noiseOffset + y * config.noiseScale);
                    gradient = Mathf.Clamp01(gradient + (noise - 0.5f) * config.noiseBorderStrength);

                    var rule = PickRule(gradient);
                    var tile = PickTile(rule, x, y);
                    var position = origin + new Vector3Int(x, y, 0);

                    targetTilemap.SetTile(position, tile);
                }
            }

            if (config.pillarHeight > 0)
                GeneratePillar();

            Debug.Log($"[IsometricGradientTilemapGenerator] Generated {size}x{size} (seed: {config.seed}).");

            if (autoFitCameraAfterGenerate && cameraAutoFitter != null)
                cameraAutoFitter.FitToTilemap();
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
            leftPillarTilemap?.ClearAllTiles();
            rightPillarTilemap?.ClearAllTiles();
            Debug.Log("[IsometricGradientTilemapGenerator] Tilemap cleared.");
        }

        private void GeneratePillar()
        {
            leftPillarTilemap?.ClearAllTiles();
            rightPillarTilemap?.ClearAllTiles();

            for (var z = 1; z <= config.pillarHeight; z++)
            {
                if (config.tileSet.leftWallTile != null && leftPillarTilemap != null)
                    for (var y = 0; y < size; y++)
                        leftPillarTilemap.SetTile(origin + new Vector3Int(0, y, -z), config.tileSet.leftWallTile);

                if (config.tileSet.rightWallTile != null && rightPillarTilemap != null)
                    for (var x = 0; x < size; x++)
                        rightPillarTilemap.SetTile(origin + new Vector3Int(x, 0, -z), config.tileSet.rightWallTile);
            }
        }

        private TilemapTileSet.TileRule PickRule(float gradient)
        {
            var bestWeight = -1f;
            var bestIndex = 0;
            var rules = config.tileSet.tileRules;

            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
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

            return rules[bestIndex];
        }

        private TileBase PickTile(TilemapTileSet.TileRule rule, int x, int y)
        {
            if (rule.variants == null || rule.variants.Count == 0)
                return rule.tile;

            var random01 = HashTo01(config.seed, x, y, 83);
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
