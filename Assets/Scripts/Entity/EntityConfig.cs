using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace Entity
{
    public enum FeatureType
    {
        None,
        Bomb,
        Crystal,
    }

    public enum EntityKind
    {
        None,
        Slime,
        Skeleton,
        Demon,
        Crystal,
    }

    [CreateAssetMenu(fileName = "EntityConfig", menuName = "RPG/Entity Config")]
    public class EntityConfig : ScriptableObject
    {
        public LocalizedString entityName = new();
        public LocalizedString description = new();
        public Sprite icon;
        public BigDouble maxHP;

        [Header("Rewards and Progression")]
        public EntityKind entityKind;
        public BigDouble shardDrop;
        public BigDouble goldDrop;
        public BigDouble xpReward = BigDouble.One;
        public bool countsAsEnemyKill = true;

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

        private void OnValidate()
        {
            maxHP = BigDoubleMath.SanitizeNonNegativeInteger(maxHP, BigDouble.One);
            if (maxHP < BigDouble.One)
                maxHP = BigDouble.One;

            shardDrop = BigDoubleMath.SanitizeNonNegativeInteger(shardDrop, BigDouble.Zero);
            goldDrop = BigDoubleMath.SanitizeNonNegativeInteger(goldDrop, BigDouble.Zero);
            xpReward = BigDoubleMath.SanitizeNonNegativeInteger(xpReward, BigDouble.Zero);
        }
    }
}
