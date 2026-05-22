using Core.Settings;
using Reflex.Core;
using UnityEngine;

namespace Reflex
{
    public class RootInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(
                GameSettingsServiceLocator.Instance,
                new[] { typeof(GameSettingsService) }
            );
        }
    }
}
