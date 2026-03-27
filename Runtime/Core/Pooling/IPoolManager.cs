using UnityEngine;

namespace Mosambi.Core.Pooling
{
    public interface IPoolManager
    {
        GameObject Spawn(string poolId, Vector3 position, Quaternion rotation);
        void Despawn(string poolId, GameObject obj);

        // The new SO-driven methods
        void LoadPoolsFromSO(PoolManifestSO poolData);
        void UnloadPoolsFromSO(PoolManifestSO poolData);
    }
}