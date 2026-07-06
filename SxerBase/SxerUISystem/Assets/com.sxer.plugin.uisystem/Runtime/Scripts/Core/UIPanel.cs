
using Cysharp.Threading.Tasks;
using Sxer.Plugin.UISystem.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Sxer.Plugin.UISystem.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : UIElement, IUIView
    {
        public CanvasGroup CanvasGroup { get; private set; }

        // 可选的通用退出按钮 (通过Inspector拖拽或代码绑定)
        [SerializeField] protected Button btnClose;

        protected override void Awake()
        {
            base.Awake();
            CanvasGroup = GetComponent<CanvasGroup>();
            if (btnClose != null)
            {
                btnClose.onClick.AddListener(() => UIManager.Instance.ClosePanelAsync(this.GetType()).Forget());
            }
        }

        // --- 核心生命周期 ---
        public virtual void OnInit() { }
        public virtual void OnOpen() { }
        public virtual void OnClose() { }

        // 实现 IUIView (配合UniTask与过渡动画)
        public virtual void Show() { CanvasGroup.interactable = true; CanvasGroup.alpha = 1; }
        public virtual void Hide() { CanvasGroup.interactable = false; CanvasGroup.alpha = 0; }
    }




    /// <summary>
    /// 带数据类型的UI面板基类，用于严格限制打开面板时传入的数据类型
    /// </summary>
    public abstract class UIPanel<TData> : UIPanel
    {
        // 密封父类的无参OnOpen，防止外部意外调用或子类错误重写
        public sealed override void OnOpen() { }

        // 强制子类实现带强类型参数的OnOpen
        public abstract void OnOpen(TData data);
    }


}