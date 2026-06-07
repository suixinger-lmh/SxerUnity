
using System;

namespace Sxer.Plugin.UISystem
{
    public interface IUIAnimation
    {
    }


    // 1. 负责生命周期的过渡动画接口
    public interface IUITransitionAnimation: IUIAnimation
    {
        void PlayEnter(Action onComplete = null);
        void PlayExit(Action onComplete = null);
    }

    // 2. 负责持续循环的状态动画接口
    public interface IUILoopAnimation: IUIAnimation
    {
        void StartLoop();
        void StopLoop();
    }

    // 3. 负责临时触发的单次反馈动画接口（如抖动、闪烁）
    public interface IUIFeedbackAnimation: IUIAnimation
    {
        // 通过 string 或 Enum 触发特定的反馈
        void PlayFeedback(string feedbackName, Action onComplete = null);
    }
}