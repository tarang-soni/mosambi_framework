using UnityEngine;
using DG.Tweening;
using System;
using Mosambi.UI;

namespace Game.UI.Animations
{
    [RequireComponent(typeof(RectTransform))]
    public class SlideUIAnimator : MonoBehaviour, IViewAnimator
    {
        public enum SlideDirection { Left, Right, Top, Bottom }

        [Header("Slide Settings")]
        [Tooltip("Which direction should the panel slide in from?")]
        [SerializeField] private SlideDirection direction = SlideDirection.Right;
        [SerializeField] private float duration = 0.35f;
        
        [Header("Easing")]
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Vector2 offscreenPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Cache the center of the screen layout
            originalPosition = rectTransform.anchoredPosition;
            CalculateOffscreenPosition();
        }

        private void CalculateOffscreenPosition()
        {
            // 2500 is a safe offset to ensure it fully clears a standard 1080x1920 mobile screen.
            // It will slide in from this position.
            float offset = 2500f; 

            switch (direction)
            {
                case SlideDirection.Left:   offscreenPosition = originalPosition + new Vector2(-offset, 0); break;
                case SlideDirection.Right:  offscreenPosition = originalPosition + new Vector2(offset, 0); break;
                case SlideDirection.Top:    offscreenPosition = originalPosition + new Vector2(0, offset); break;
                case SlideDirection.Bottom: offscreenPosition = originalPosition + new Vector2(0, -offset); break;
            }
        }

        public void AnimateShow(CanvasGroup canvasGroup, Action onComplete = null)
        {
            DOTween.Kill(this);

            // 1. Snap panel to the offscreen starting position
            rectTransform.anchoredPosition = offscreenPosition;
            
            // 2. Ensure it is fully opaque (in case a different animator faded it out previously)
            canvasGroup.alpha = 1f; 

            // 3. Slide it to the original layout position
            rectTransform.DOAnchorPos(originalPosition, duration)
                .SetEase(showEase)
                .SetId(this)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateHide(CanvasGroup canvasGroup, Action onComplete = null)
        {
            DOTween.Kill(this);

            // 1. Slide it back off the screen
            rectTransform.DOAnchorPos(offscreenPosition, duration)
                .SetEase(hideEase)
                .SetId(this)
                .OnComplete(() => {
                    // Failsafe: Once it is invisible, snap it back to the center 
                    // so your layout in the Editor doesn't get permanently ruined.
                    rectTransform.anchoredPosition = originalPosition; 
                    onComplete?.Invoke();
                });
        }
    }
}