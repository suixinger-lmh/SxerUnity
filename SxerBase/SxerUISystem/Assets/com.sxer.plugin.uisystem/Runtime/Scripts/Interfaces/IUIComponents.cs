namespace Sxer.Plugin.UISystem.Interfaces
{
    // 标签页系统
    public interface ITabGroup { void SwitchTab(int index); }
    public interface ITabPage { void OnTabSelected(); void OnTabDeselected(); }

    // 独立视口 / 独立弹窗
    public interface IUIView
    {
        void Show();
        void Hide();
    }

    // 参数控制
    public interface IUIParameter<T>
    {
        T Value { get; set; }
        void OnValueChanged(T newValue);
    }
}