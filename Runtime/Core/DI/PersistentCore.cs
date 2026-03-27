using UnityEngine;
using VContainer.Unity;

namespace Mosambi.Core
{
    public class PersistentCore : MonoBehaviour
    {
        private static PersistentCore _instance;

        private void Awake()
        {
            // Standard Singleton protection
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            // Move this object to the "Immortal" space
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[CORE] Persistent Core Initialized and marked DontDestroyOnLoad.");
        }
    }
}