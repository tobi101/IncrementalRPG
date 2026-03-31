using Core;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
using Core.Save;
using Entity;
using IncrementalRPG.Scripts.Core;
using IncrementalRPG.Scripts.Reflex;
using IncrementalRPG.Scripts.AudioManager;
using Model;
using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using UnityEngine.Tilemaps;
using Resolution = Reflex.Enums.Resolution;

namespace Reflex
{
    public class GameSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private DungeonConfig _dungeonConfig;
        
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private SpawnTable _spawnTable;
        [SerializeField] private DamageZoneConfig _damageZoneConfig;
        [SerializeField] private DamageZoneView _damageZoneView;
        [SerializeField] private CoinPileView _coinPileViewPrefab;
        [SerializeField] private GoldWalletView _goldWalletView;

        [SerializeField] private AudioManager _audioManager;

        private void Awake()
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }

        public void InstallBindings(ContainerBuilder builder)
        {
            // InitializeConfigs(builder);
            
            builder.RegisterType(
                typeof(GameLoop),
                new[] { typeof(IStartable), typeof(ITickable) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(SaveService),
                new[] { typeof(SaveService) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(Player),
                new[] { typeof(Player) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterValue(_audioManager);

            // var tileGrid = new TileGrid(_groundTilemap);
            // builder.RegisterValue(tileGrid, new[] { typeof(TileGrid) });
            
            // builder.RegisterType(
            //     typeof(PoolManager),
            //     new[] { typeof(IService), typeof(PoolManager) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            //
            // builder.RegisterType(
            //     typeof(SpawnService),
            //     new[] { typeof(IService), typeof(SpawnService) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            //
            // builder.RegisterType(
            //     typeof(DamageZone),
            //     new[] { typeof(IService), typeof(DamageZone) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            //
            // builder.RegisterType(
            //     typeof(GoldWallet),
            //     new[] { typeof(IService), typeof(ISaveable), typeof(GoldWallet) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            //
            // builder.RegisterType(
            //     typeof(CoinSpawnService),
            //     new[] { typeof(IService) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            //
            // builder.RegisterType(
            //     typeof(CoinCollector),
            //     new[] { typeof(IService) },
            //     Lifetime.Singleton,
            //     Resolution.Lazy
            // );
            
            // builder.RegisterValue(_damageZoneView, new[] { typeof(DamageZoneView) });
            // builder.RegisterValue(_coinPileViewPrefab, new[] { typeof(CoinPileView) });
            // builder.RegisterValue(_goldWalletView, new[] { typeof(GoldWalletView) });
        }

        public void Exit()
        {
            Application.Quit();
        }

        private void InitializeConfigs(ContainerBuilder builder)
        {
            builder.RegisterValue(_dungeonConfig, new[] { typeof(DungeonConfig) });
            builder.RegisterValue(_damageZoneConfig, new[] { typeof(DamageZoneConfig) });
        }
    }
}
