using UnityEngine;

namespace Mosambi.Core.Pooling
{
    [CreateAssetMenu(fileName = "NewPoolData", menuName = "Mosambi/Pooling/Pool Data")]
    public class PoolDataSO : ScriptableObject
    {
        public string poolId; // e.g., "Gem_Red"
        public GameObject prefab;
        
        [Range(1, 100)] 
        public int initialSize = 10;
        
        public bool isExpandable = true;
    }
}