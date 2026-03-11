using Reflex.Core;
using Reflex.Enums;
using IncrementalRPG.Scripts.Core;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace IncrementalRPG.Scripts.Reflex
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
        }
    }
}
