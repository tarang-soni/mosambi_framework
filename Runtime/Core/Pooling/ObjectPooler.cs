using UnityEngine;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;

namespace Mosambi.Core.Pooling
{
    public class ObjectPooler : MonoBehaviour
    {
        [SerializeField] private PoolType type;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        public PoolType Type => type;

        private IObjectResolver _container;
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        [Inject]
        public void Construct(IObjectResolver container) => _container = container;

        public void Initialize()
        {
            for (int i = 0; i < initialSize; i++) CreateNewObject();
        }

        private void CreateNewObject()
        {
            GameObject obj = _container.Instantiate(prefab);
            obj.transform.SetParent(this.transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        public GameObject Get()
        {
            if (_pool.Count == 0) CreateNewObject();
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(this.transform);
            _pool.Enqueue(obj);
        }
    }
}