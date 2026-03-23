using UnityEngine;

namespace Entity
{
    public abstract class Entity
    {
        public EntityConfig Config { get; }
        public Vector2Int TileCoord { get; protected set; }

        protected Entity(EntityConfig config, Vector2Int tileCoord)
        {
            Config = config;
            TileCoord = tileCoord;
        }
    }
}
