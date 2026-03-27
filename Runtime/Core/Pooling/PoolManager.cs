using System.Collections.Generic;
using UnityEngine;

namespace Mosambi.Core.Pooling
{
    public class PoolManager : MonoBehaviour, IPoolManager
    {
        // Stores our active pools. String ID -> Queue of available GameObjects
        private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabRegistry = new Dictionary<string, GameObject>();

        public void CreatePool(string poolId, GameObject prefab, int initialSize)
        {
            if (_poolDictionary.ContainsKey(poolId))
            {
                Debug.LogWarning($"[PoolManager] Pool {poolId} already exists!");
                return;
            }

            _poolDictionary.Add(poolId, new Queue<GameObject>());
            _prefabRegistry.Add(poolId, prefab);

            // Create an empty GameObject to keep the hierarchy clean
            GameObject poolParent = new GameObject($"Pool_{poolId}");
            poolParent.transform.SetParent(this.transform);

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, poolParent.transform);
                obj.SetActive(false);
                _poolDictionary[poolId].Enqueue(obj);
            }
        }

        public void DestroyPool(string poolId)
        {
            if (!_poolDictionary.ContainsKey(poolId)) return;

            // Destroy all objects in the queue to free up RAM
            foreach (var obj in _poolDictionary[poolId])
            {
                if (obj != null) Destroy(obj);
            }

            _poolDictionary.Remove(poolId);
            _prefabRegistry.Remove(poolId);

            // Clean up the parent container
            Transform oldParent = transform.Find($"Pool_{poolId}");
            if (oldParent != null) Destroy(oldParent.gameObject);
        }
        // Inside your PoolManager class...

        public void LoadPoolsFromSO(PoolManifestSO poolData)
        {
            if (poolData == null) return;

            foreach (var config in poolData.requiredPools)
            {
                // We reuse the CreatePool logic we already wrote!
                CreatePool(config.poolId, config.prefab, config.initialSize);
            }

            Debug.Log($"[PoolManager] Successfully loaded {poolData.requiredPools.Count} pools from SO.");
        }

        public void UnloadPoolsFromSO(PoolManifestSO poolData)
        {
            if (poolData == null) return;

            foreach (var config in poolData.requiredPools)
            {
                DestroyPool(config.poolId);
            }
        }
        public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(poolId)) return null;

            GameObject objToSpawn;

            // If the pool is empty, expand it dynamically
            if (_poolDictionary[poolId].Count == 0)
            {
                GameObject poolParent = transform.Find($"Pool_{poolId}").gameObject;
                objToSpawn = Instantiate(_prefabRegistry[poolId], poolParent.transform);
            }
            else
            {
                objToSpawn = _poolDictionary[poolId].Dequeue();
            }

            objToSpawn.SetActive(true);
            objToSpawn.transform.SetPositionAndRotation(position, rotation);
            return objToSpawn;
        }

        public void Despawn(string poolId, GameObject obj)
        {
            if (!_poolDictionary.ContainsKey(poolId)) return;

            obj.SetActive(false);
            _poolDictionary[poolId].Enqueue(obj);
        }
    }
}