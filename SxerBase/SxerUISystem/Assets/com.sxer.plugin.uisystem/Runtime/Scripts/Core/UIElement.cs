using UnityEngine;
using Cysharp.Threading.Tasks;
using Sxer.Plugin.UISystem;
using Sxer.Plugin.UISystem.Interfaces;

namespace Sxer.Plugin.UISystem.Core
{

    [RequireComponent(typeof(RectTransform))]
    public abstract class UIElement : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        // 拥有的动画能力（通过GetComponent获取，解耦具体实现）
        protected IUITransitionAnimation transitionAnim;
        protected IUILoopAnimation loopAnim;

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            transitionAnim = GetComponent<IUITransitionAnimation>();
            loopAnim = GetComponent<IUILoopAnimation>();
        }
    }
}