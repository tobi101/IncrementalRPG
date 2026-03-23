using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _inactive = new();

        public ObjectPool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public T Get()
        {
            if (_inactive.Count > 0)
            {
                var obj = _inactive.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            return Object.Instantiate(_prefab, _parent);
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _inactive.Enqueue(obj);
        }
    }
}
