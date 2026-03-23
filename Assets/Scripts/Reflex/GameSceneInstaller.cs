using Core;
using Core.Gameplay;
using Entity;
using IncrementalRPG.Scripts.Core;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using UnityEngine.Tilemaps;
using Resolution = Reflex.Enums.Resolution;

namespace Reflex
{
    public class GameSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private EntityConfig[] _spawnableConfigs;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterType(
                typeof(GameLoop),
                new[] { typeof(IStartable), typeof(ITickable) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterValue(_groundTilemap, new[] { typeof(Tilemap) });
            builder.RegisterValue(_spawnableConfigs, new[] { typeof(EntityConfig[]) });

            builder.RegisterType(
                typeof(PoolManager),
                new[] { typeof(IService), typeof(PoolManager) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(SpawnService),
                new[] { typeof(IService) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
        }
    }
}
