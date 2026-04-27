using UnityEngine;

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
        public string entityName;
        public int maxHP;
        public int goldDrop;
        public FeatureType featureType;
        public GameObject viewPrefab;
        public AudioClip damageSound;
    }
}
