using UnityEngine;

namespace Core.Test
{
    [CreateAssetMenu(fileName = "SkillNode", menuName = "SkillTree/Node")]
    public class SkillNodeConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public int MaxLevel = 1;
        public int GoldCostPerLevel = 100;

        [Tooltip("Позиция узла на канвасе дерева (в пикселях)")]
        public Vector2 Position;

        [Tooltip("Узлы, которые должны быть прокачаны до PrerequisiteMinLevel")]
        public SkillNodeConfig[] Prerequisites;
        public int PrerequisiteMinLevel = 1;

        [Tooltip("Эффекты умножаются на текущий уровень узла")]
        public StatModifier[] EffectsPerLevel;
    }
}
