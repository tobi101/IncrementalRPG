using System;
using System.Linq;
using Core.Gameplay.Dungeon;
using Core.Items;
using UnityEngine;

namespace Core.Classes
{
    [CreateAssetMenu(menuName = "RPG/Classes/Catalog")]
    public sealed class ClassCatalog : ScriptableObject
    {
        public DungeonConfig introductionDungeon;
        public ItemDefinition runeKey;
        public AttackDefinition defaultManualAttack;
        public ClassDefinition[] classes = Array.Empty<ClassDefinition>();

        public ClassDefinition FindClass(string id) => classes.FirstOrDefault(c => c != null && c.id == id);
        public ClassDefinition OwnerOf(AttackDefinition attack) => classes.FirstOrDefault(c => c != null &&
            c.nodes.Any(n => n != null && n.reward == ClassNodeReward.UnlockAttack && n.attack == attack));
        public AttackDefinition FindAttack(string id) => string.IsNullOrEmpty(id) ? null :
            defaultManualAttack != null && defaultManualAttack.id == id ? defaultManualAttack :
            classes.Where(c => c != null).SelectMany(c => c.nodes).Where(n => n != null && n.attack != null)
                .Select(n => n.attack).FirstOrDefault(a => a.id == id);
    }
}
