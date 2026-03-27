using System;
using UnityEngine;
using Mosambi.Core;

namespace Mosambi.Events
{
    [CreateAssetMenu(menuName = "Mosambi/Events/Game State Channel", fileName = "GameStateChannel")]
    public class GameStateChannelSO : ScriptableObject
    {
        public event Action<GameState> OnStateChanged;

        public void RaiseEvent(GameState newState)
        {
            OnStateChanged?.Invoke(newState);
        }
    }
}