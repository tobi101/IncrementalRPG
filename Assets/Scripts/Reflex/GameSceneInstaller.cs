using Core;
using Core.Gameplay;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Core;
using Reflex.Enums;
using Resolution = Reflex.Enums.Resolution;
using UnityEngine;

namespace Reflex
{
    public class GameSceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterType(
                typeof(GameLoop),
                new[] { typeof(IStartable), typeof(ITickable) },
                Lifetime.Singleton,
                Resolution.Lazy
            );

            builder.RegisterType(
                typeof(SpawnService),
                new[] { typeof(IStartable), typeof(ITickable) },
                Lifetime.Singleton,
                Resolution.Lazy
            );
        }
    }
}
