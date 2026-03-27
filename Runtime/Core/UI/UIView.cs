using UnityEngine;
using System;

namespace Mosambi.UI
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        protected Canvas canvas;
        protected CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [Tooltip("Drag a MonoBehaviour or ScriptableObject that implements IViewAnimator here. If left blank, it will auto-search this GameObject.")]
        [SerializeField] private UnityEngine.Object animatorObject;

        // The actual interface we use in code, hidden from the inspector
        private IViewAnimator animator;

        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();

            // 1. Try to cast the object assigned in the inspector
            if (animatorObject != null)
            {
                animator = animatorObject as IViewAnimator;
                if (animator == null)
                {
                    Debug.LogError($"[UIView] The object assigned to {gameObject.name} does not implement IViewAnimator!");
                }
            }

            // 2. Fallback: If inspector is empty, check if an animator script is attached to this exact GameObject
            if (animator == null)
            {
                animator = GetComponent<IViewAnimator>();
            }
        }

        public virtual void Show()
        {
            canvas.enabled = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (animator != null)
            {
                animator.AnimateShow(canvasGroup);
            }
            else
            {
                canvasGroup.alpha = 1f;
            }
        }

        public virtual void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (animator != null)
            {
                animator.AnimateHide(canvasGroup, () => canvas.enabled = false);
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvas.enabled = false;
            }
        }
    }
}