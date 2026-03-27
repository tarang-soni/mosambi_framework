using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Mosambi.Core.Loading;
using DG.Tweening; // Import DOTween

namespace Mosambi.Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private CanvasGroup canvasGroup;

        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;

            _sceneLoader.OnLoadStarted += ShowScreen;
            _sceneLoader.OnLoadCompleted += HideScreen;

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            if (_sceneLoader != null)
            {
                _sceneLoader.OnLoadStarted -= ShowScreen;
                _sceneLoader.OnLoadCompleted -= HideScreen;
            }

            // Safety: kill the tween if the game quits while loading
            progressBar.DOKill();
            canvasGroup.DOKill();
        }

        private void ShowScreen()
        {
            canvasGroup.blocksRaycasts = true;
            progressBar.value = 0f;

            // Fade the background in instantly or over 0.2s
            canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);

            // THE DOTWEEN MAGIC: 
            // Smoothly animate the slider to 1 over exactly 2.5 seconds.
            // SetEase(Ease.Linear) makes it a constant speed.
            // SetUpdate(true) ensures it plays even if the game is paused!
            progressBar.DOValue(1f, SceneLoader.MinimumLoadTime)
                       .SetEase(Ease.Linear)
                       .SetUpdate(true);
        }

        private void HideScreen()
        {
            canvasGroup.blocksRaycasts = false;

            // Fade the loading screen out smoothly so it isn't jarring
            canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
        }
    }
}