using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "TilemapTileSet", menuName = "RPG/Tilemap Tile Set")]
    public class TilemapTileSet : ScriptableObject
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

        [Header("Floor Tiles")]
        public List<TileRule> tileRules = new();

        [Header("Pillar Tiles")]
        public TileBase leftWallTile;
        public TileBase rightWallTile;
    }
}
