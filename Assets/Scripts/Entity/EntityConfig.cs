using UnityEngine;

namespace Entity
{
    [CreateAssetMenu(fileName = "EntityConfig", menuName = "RPG/Entity Config")]
    public class EntityConfig : ScriptableObject
    {
        public string entityName;
        public int maxHP;
        public int goldDrop;
        public bool canCoexistWithOthers;
        public GameObject viewPrefab;
        public AudioClip damageSound;
    }
}
