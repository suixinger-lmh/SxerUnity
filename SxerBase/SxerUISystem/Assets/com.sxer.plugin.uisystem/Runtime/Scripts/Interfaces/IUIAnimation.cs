using Cysharp.Threading.Tasks;

namespace Sxer.Plugin.UISystem.Interfaces
{

    public interface IUIAnimation { }

    // 1. 过渡动画（生命周期强绑定）
    public interface IUITransitionAnimation : IUIAnimation
    {
        // 提供给外部等待动画播放完毕
        UniTask PlayEnterAsync();
        UniTask PlayExitAsync();
    }

    // 2. 循环/状态动画（状态控制）
    public interface IUILoopAnimation : IUIAnimation
    {
        void StartLoop();
        void StopLoop();
    }

    // 3. 瞬时反馈动画（事件触发）
    public interface IUIFeedbackAnimation : IUIAnimation
    {
        void PlayFeedback(); // 一次性触发，如点击抖动、飘字
    }
}