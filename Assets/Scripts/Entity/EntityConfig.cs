using UnityEngine;
using UnityEngine.Localization;

namespace Entity
{
    public enum FeatureType
    {
        None,
        Bomb,
    }

    [CreateAssetMenu(fileName = "EntityConfig", menuName = "RPG/Entity Config")]
    public class EntityConfig : ScriptableObject
    {
        public LocalizedString entityName = new();
        public LocalizedString description = new();
        public Sprite icon;
        public int maxHP;
        public int goldDrop;
        public FeatureType featureType;
        public GameObject viewPrefab;

        [Header("Gameplay Bounds")]
        [Min(0f)] public float damageZoneHitRadius = 0.25f;

        [Header("Debug")]
        public bool drawDamageZoneHitAreaGizmo = true;
        public Color damageZoneHitAreaGizmoColor = new Color(1f, 0.85f, 0f, 0.8f);

        [Header("Audio")]
        public AudioClip damageSound;
        public AudioClip[] deathSounds;
    }
}
