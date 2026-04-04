using Core;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
using Core.Save;
using Core.TestSkillTree;
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
        [SerializeField] private DungeonList _dungeonList;
        [SerializeField] private DamageZoneConfig _damageZoneConfig;
        [SerializeField] private SkillTreeConfig _skillTreeConfig;
        
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private IsometricGradientTilemapGenerator _isometricGradientTilemapGenerator;

        [SerializeField] private DamageZoneView _damageZoneView;


        private void Awake()
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }

        public void InstallBindings(ContainerBuilder builder)
        {
            InitializeConfigs(builder);
            
            builder.RegisterType(
                typeof(GameLoop),
                new[] { typeof(IAwakeable), typeof(IStartable), typeof(ITickable) },
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

            builder.RegisterType(
                typeof(SkillTreeService),
                new[] { typeof(ISaveable), typeof(SkillTreeService) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterValue(_audioManager);
            builder.RegisterValue(_isometricGradientTilemapGenerator);
            builder.RegisterValue(_damageZoneView, new[] { typeof(DamageZoneView) });
            
            var tileGrid = new TileGrid();
            builder.RegisterValue(tileGrid, new[] { typeof(TileGrid) });
            
            builder.RegisterType(
                typeof(PoolManager),
                new[] { typeof(IService), typeof(PoolManager) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(SpawnService),
                new[] { typeof(IService), typeof(SpawnService) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(DamageZone),
                new[] { typeof(IService), typeof(DamageZone) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            // builder.RegisterType(
            //     typeof(GoldWallet),
            //     new[] { typeof(IService), typeof(ISaveable), typeof(GoldWallet) },
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
            
            // builder.RegisterValue(_goldWalletView, new[] { typeof(GoldWalletView) });
        }

        public void Exit()
        {
            Application.Quit();
        }

        private void InitializeConfigs(ContainerBuilder builder)
        {
            builder.RegisterValue(_dungeonList, new[] { typeof(DungeonList) });
            builder.RegisterValue(_damageZoneConfig, new[] { typeof(DamageZoneConfig) });
            builder.RegisterValue(_skillTreeConfig, new[] { typeof(SkillTreeConfig) });
        }
    }
}
