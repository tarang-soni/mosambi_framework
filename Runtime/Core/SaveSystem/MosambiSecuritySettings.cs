using UnityEngine;

namespace Mosambi.Core
{
    // This lives in your Project Assets, so the Debugger can always find it
    [CreateAssetMenu(fileName = "MosambiSecuritySettings", menuName = "Mosambi/Settings/Security")]
    public class MosambiSecuritySettings : ScriptableObject
    {
        public string encryptionKey = "MosambiSuperSecretKey123456789012"; // 32 chars
        public string initializationVector = "MosambiIVVector1"; // 16 chars
        public string saveFileName = "mosambi_secure.dat";
    }
}