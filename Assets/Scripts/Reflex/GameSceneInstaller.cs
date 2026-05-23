using Core.Gameplay;
using Core.Gameplay.Bomb;
using Core.Gameplay.Dungeon;
using Core.Save;
using Core.StateMachine;
using Core.StateMachine.Features;
using Core.StateMachine.States;
using Core.TestSkillTree;
using Core.TestSkillTree.View;
using Entity;
using IncrementalRPG.Scripts.Core;
using IncrementalRPG.Scripts.Reflex;
using IncrementalRPG.Scripts.AudioManager;
using Model;
using Reflex.Core;
using Reflex.Enums;
using UI;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace Reflex
{
    public class GameSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private DungeonList _dungeonList;
        [SerializeField] private DungeonLevelTransitionConfig _levelTransitionConfig = new();
        [SerializeField] private DamageZoneConfig _damageZoneConfig;
        [SerializeField] private BombExplosionConfig _bombExplosionConfig;
        [SerializeField] private SkillTreeConfig _skillTreeConfig;
        [SerializeField] private NodeBorderColorConfig _nodeBorderColorConfig;
        
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private IsometricGradientTilemapGenerator _isometricGradientTilemapGenerator;

        [SerializeField] private DamageZoneView _damageZoneView;
        [SerializeField] private SkillTreeView _skillTreeView;
        [SerializeField] private HubView _hubView;
        [SerializeField] private MenuCanvasView _menuCanvasView;
        [SerializeField] private HudView _hudView;
        [SerializeField] private PauseMenuController _pauseMenuController;
        [SerializeField] private SessionEndPopupView _sessionEndPopupView;


        private void Awake()
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }

        public void InstallBindings(ContainerBuilder builder)
        {
            InitializeConfigs(builder);
            
            builder.RegisterType(
                typeof(GameStateMachine),
                new[] { typeof(IAwakeable), typeof(IStartable), typeof(ITickable), typeof(GameStateMachine) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(HubState),
                new[] { typeof(IGameState), typeof(HubState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(HubFeature),
                new[] { typeof(IGameFeature), typeof(HubFeature) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(GameplayState),
                new[] { typeof(IGameState), typeof(GameplayState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(SkillTreeMenuState),
                new[] { typeof(IGameState), typeof(SkillTreeMenuState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(GameplayFeature),
                new[] { typeof(IGameFeature), typeof(GameplayFeature) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(GameplayInputBlocker),
                new[] { typeof(GameplayInputBlocker) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(DungeonSelectionService),
                new[] { typeof(DungeonSelectionService), typeof(ISaveable) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(SkillTreeFeature),
                new[] { typeof(IGameFeature), typeof(SkillTreeFeature) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(BarracksState),
                new[] { typeof(IGameState), typeof(BarracksState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(MineState),
                new[] { typeof(IGameState), typeof(MineState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(CraftState),
                new[] { typeof(IGameState), typeof(CraftState) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterType(
                typeof(SaveService),
                new[] { typeof(SaveService) },
                Lifetime.Singleton,
                Resolution.Eager
            );

            builder.RegisterType(
                typeof(Player),
                new[] { typeof(ISaveable), typeof(Player) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(SkillTreeService),
                new[] { typeof(ISaveable), typeof(SkillTreeService) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
            
            builder.RegisterValue(ResolveAudioManager());
            builder.RegisterValue(_isometricGradientTilemapGenerator);
            
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

            builder.RegisterType(
                typeof(BombExplosionService),
                new[] { typeof(IService), typeof(BombExplosionService) },
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
            builder.RegisterValue(_damageZoneView, new[] { typeof(DamageZoneView) });
            builder.RegisterValue(_skillTreeView, new[] { typeof(SkillTreeView) });
            builder.RegisterValue(_hubView, new[] { typeof(HubView) });
            builder.RegisterValue(_menuCanvasView, new[] { typeof(MenuCanvasView) });
            builder.RegisterValue(_hudView, new[] { typeof(HudView) });
            builder.RegisterValue(_pauseMenuController, new[] { typeof(PauseMenuController) });
            builder.RegisterValue(_sessionEndPopupView, new[] { typeof(SessionEndPopupView) });
        }

        public void Exit()
        {
            Application.Quit();
        }

        private void InitializeConfigs(ContainerBuilder builder)
        {
            if (_levelTransitionConfig == null)
                _levelTransitionConfig = new DungeonLevelTransitionConfig();

            builder.RegisterValue(_dungeonList, new[] { typeof(DungeonList) });
            builder.RegisterValue(_levelTransitionConfig, new[] { typeof(DungeonLevelTransitionConfig) });
            builder.RegisterValue(_damageZoneConfig, new[] { typeof(DamageZoneConfig) });
            builder.RegisterValue(_bombExplosionConfig, new[] { typeof(BombExplosionConfig) });
            builder.RegisterValue(_skillTreeConfig, new[] { typeof(SkillTreeConfig) });
            builder.RegisterValue(_nodeBorderColorConfig, new[] { typeof(NodeBorderColorConfig) });
        }

        private AudioManager ResolveAudioManager()
        {
            var audioManager = AudioManager.Resolve(_audioManager);
            if (audioManager != null)
                return audioManager;

            Debug.LogWarning("[GameSceneInstaller] AudioManager was not found. Creating runtime fallback. Start from BootstrapScene or assign _audioManager to use configured audio clips.");

            var audioManagerObject = new GameObject("AudioManager Runtime Fallback");
            audioManager = audioManagerObject.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObject);
            return audioManager;
        }
    }
}
