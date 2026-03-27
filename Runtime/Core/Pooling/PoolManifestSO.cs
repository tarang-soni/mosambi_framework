using System.Collections.Generic;
using UnityEngine;

namespace Mosambi.Core.Pooling
{
    [CreateAssetMenu(fileName = "PoolManifest", menuName = "Mosambi/Pooling/Pool Manifest")]
    public class PoolManifestSO : ScriptableObject
    {
        public List<PoolDataSO> requiredPools = new List<PoolDataSO>();
    }
}