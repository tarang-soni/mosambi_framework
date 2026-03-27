using System;

namespace Mosambi.Core
{
    [Serializable]
    public class GameSaveData
    {
        // Default values for a brand new player
        public bool isSoundOn = true;
        public bool isHapticsOn = true;
        
        // Example of what you can easily add later without breaking the system:
        // public int totalCoins = 0;
        // public bool noAdsPurchased = false;
    }
}