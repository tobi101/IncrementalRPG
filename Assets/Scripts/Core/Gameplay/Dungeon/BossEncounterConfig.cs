using Entity;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "BossEncounter", menuName = "RPG/Boss Encounter")]
    public sealed class BossEncounterConfig : ScriptableObject
    {
        public EntityConfig boss;
        public EntityConfig totem;
        [Min(0.1f)] public float restoreDelay = 6f;

        [Header("Shield effects")]
        public Material shieldMaterial;
        public Material linkMaterial;
        public Vector2 shieldCenterOffset = new(0f, 1.2f);
        public Vector2 shieldRadii = new(1.55f, 2f);
        public Vector2 totemLinkOffset = new(0.08f, 0.46f);

        public bool IsPlayable => boss != null && boss.viewPrefab != null && totem != null && totem.viewPrefab != null
                                  && restoreDelay > 0f && float.IsFinite(restoreDelay)
                                  && shieldMaterial != null && linkMaterial != null && shieldRadii.x > 0f && shieldRadii.y > 0f;
    }
}
