using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Pooling
{
    public sealed class GameObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> idle = new Stack<T>();

        public GameObjectPool(T prefab, Transform parent, int prewarm)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < prewarm; i++)
                idle.Push(CreateIdle());
        }

        public int IdleCount => idle.Count;

        public int TotalCreated { get; private set; }

        public T Get(Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            T instance = idle.Count > 0 ? idle.Pop() : CreateIdle();

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null) return;
            if (!instance.gameObject.activeSelf) return;

            instance.gameObject.SetActive(false);

            if (parent != null) instance.transform.SetParent(parent, false);

            idle.Push(instance);
        }

        private T CreateIdle()
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);

            TotalCreated++;

            return instance;
        }
    }
}
