using Cysharp.Threading.Tasks; // Or System.Threading.Tasks if you haven't swapped yet
using System;

namespace Mosambi.Core.Loading
{
    public interface ISceneLoader
    {
        // Add these three events
        event Action OnLoadStarted;
        event Action<float> OnLoadProgress;
        event Action OnLoadCompleted;

        UniTask LoadSceneAsync(string sceneName);
    }
}