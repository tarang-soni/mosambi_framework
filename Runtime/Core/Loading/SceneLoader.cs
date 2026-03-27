using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Mosambi.Core.Loading
{
    public class SceneLoader : ISceneLoader
    {
        private readonly LifetimeScope _parentScope;
        private bool _isLoading;

        // The master lock timer
        public const float MinimumLoadTime = 2.5f;

        public event Action OnLoadStarted;
        public event Action<float> OnLoadProgress; // (We actually won't even need this anymore!)
        public event Action OnLoadCompleted;

        [Inject]
        public SceneLoader(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;

            OnLoadStarted?.Invoke();
            await UniTask.Yield(); // 1 frame delay to let the UI turn on

            // Start the strict stopwatch
            float startTime = Time.unscaledTime;

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var op = SceneManager.LoadSceneAsync(sceneName);
                op.allowSceneActivation = false;

                // Wait for Unity to finish loading the heavy assets into memory (stops at 0.9)
                await UniTask.WaitUntil(() => op.progress >= 0.9f);

                // Check the stopwatch. Did it load too fast?
                float elapsedTime = Time.unscaledTime - startTime;
                if (elapsedTime < MinimumLoadTime)
                {
                    // Force the thread to wait the remaining time
                    float timeRemaining = MinimumLoadTime - elapsedTime;
                    await UniTask.Delay(TimeSpan.FromSeconds(timeRemaining), ignoreTimeScale: true);
                }

                // Door is unlocked.
                op.allowSceneActivation = true;
                await UniTask.WaitUntil(() => op.isDone);
            }

            _isLoading = false;
            OnLoadCompleted?.Invoke();
        }
    }
}