using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Core.Classes
{
    [CreateAssetMenu(menuName = "RPG/Classes/Class")]
    public sealed class ClassDefinition : ScriptableObject
    {
        public string id;
        public LocalizedString displayName = new();
        public LocalizedString description = new();
        public Color color = Color.white;
        public Sprite emblem;
        public Sprite banner;
        public Sprite pattern;
        public Sprite sigil;
        public Sprite emblemAura;
        public Sprite emblemRunes;
        public Sprite emblemHalo;
        [Min(1)] public int unlockKeyCost = 1;
        public ClassNodeDefinition[] nodes = Array.Empty<ClassNodeDefinition>();
    }
}
