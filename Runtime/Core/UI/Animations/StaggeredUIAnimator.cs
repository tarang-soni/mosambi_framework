using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Mosambi.UI;

namespace Game.UI.Animations
{
    public class StaggeredUIAnimator : MonoBehaviour, IViewAnimator
    {
        [Header("Canvas Fade")]
        [SerializeField] private float backgroundFadeDuration = 0.2f;

        [Header("Individual Elements")]
        [Tooltip("Drag the RectTransforms you want to animate in order.")]
        [SerializeField] private RectTransform[] elementsToStagger;
        
        [Header("Animation Settings")]
        [SerializeField] private float staggerDelay = 0.1f;
        [SerializeField] private float elementAnimDuration = 0.4f;
        [SerializeField] private Vector2 slideOffset = new Vector2(0, -150f); // Slides up from below

        // The Failsafe Cache
        private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
        private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();

        private void Awake()
        {
            // Cache the exact layout positions so we never break the UI 
            // if the animation is interrupted halfway through.
            foreach (var el in elementsToStagger)
            {
                if (el == null) continue;
                originalPositions[el] = el.anchoredPosition;
                originalScales[el] = el.localScale;
            }
        }

        public void AnimateShow(CanvasGroup mainCanvasGroup, Action onComplete = null)
        {
            // Kill any currently running animations on this panel
            DOTween.Kill(this);

            Sequence seq = DOTween.Sequence();
            seq.SetId(this); // Tag it so we can kill it later

            // 1. Immediately fade in the invisible canvas so we can see the elements moving
            mainCanvasGroup.alpha = 0f;
            seq.Append(mainCanvasGroup.DOFade(1f, backgroundFadeDuration));

            // 2. Setup and stagger the elements
            for (int i = 0; i < elementsToStagger.Length; i++)
            {
                RectTransform el = elementsToStagger[i];
                if (el == null) continue;

                // Snap to starting "hidden" position
                el.anchoredPosition = originalPositions[el] + slideOffset;
                el.localScale = Vector3.zero;

                // Animate to original position (Ease.OutBack gives that satisfying "bouncy" pop)
                float insertTime = backgroundFadeDuration + (i * staggerDelay);
                seq.Insert(insertTime, el.DOAnchorPos(originalPositions[el], elementAnimDuration).SetEase(Ease.OutBack));
                seq.Insert(insertTime, el.DOScale(originalScales[el], elementAnimDuration).SetEase(Ease.OutBack));
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateHide(CanvasGroup mainCanvasGroup, Action onComplete = null)
        {
            DOTween.Kill(this);

            Sequence seq = DOTween.Sequence();
            seq.SetId(this);

            // 1. Reverse stagger the elements out much faster than they came in
            float fastDuration = elementAnimDuration * 0.5f;
            
            for (int i = elementsToStagger.Length - 1; i >= 0; i--)
            {
                RectTransform el = elementsToStagger[i];
                if (el == null) continue;

                float insertTime = ((elementsToStagger.Length - 1 - i) * (staggerDelay * 0.5f));

                seq.Insert(insertTime, el.DOAnchorPos(originalPositions[el] - slideOffset, fastDuration).SetEase(Ease.InBack));
                seq.Insert(insertTime, el.DOScale(Vector3.zero, fastDuration).SetEase(Ease.InBack));
            }

            // 2. Fade the canvas out at the end
            float totalAnimTime = (elementsToStagger.Length * staggerDelay * 0.5f) + fastDuration;
            seq.Insert(totalAnimTime, mainCanvasGroup.DOFade(0f, backgroundFadeDuration));

            seq.OnComplete(() => {
                // Failsafe: Reset everything to normal while invisible so it's ready for next time
                ResetToOriginalState();
                onComplete?.Invoke();
            });
        }

        private void ResetToOriginalState()
        {
            foreach (var el in elementsToStagger)
            {
                if (el == null) continue;
                el.anchoredPosition = originalPositions[el];
                el.localScale = originalScales[el];
            }
        }
    }
}