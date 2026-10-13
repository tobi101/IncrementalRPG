using UnityEngine;
using UnityEngine.Localization;

namespace Core.Classes
{
    public enum AttackSlot { Manual, Automatic, Special }

    [CreateAssetMenu(menuName = "RPG/Classes/Attack")]
    public sealed class AttackDefinition : ScriptableObject
    {
        public string id;
        public AttackSlot slot;
        public LocalizedString displayName = new();
        public LocalizedString description = new();
        public Sprite icon;
        public Sprite preview;
    }
}
