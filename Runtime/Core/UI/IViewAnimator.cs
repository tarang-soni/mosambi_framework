using UnityEngine;
using System;

namespace Mosambi.UI
{
    public interface IViewAnimator
    {
        void AnimateShow(CanvasGroup canvasGroup, Action onComplete = null);
        void AnimateHide(CanvasGroup canvasGroup, Action onComplete = null);
    }
}