using Sxer.Plugin.UISystem.Interfaces;
using UnityEngine;

namespace Sxer.Plugin.UISystem
{
    public class TabPage : MonoBehaviour, ITabPage
    {
        public void OnTabSelected()
        {
            // 页面被选中时触发：可以播放动画、请求网络数据等
            // Debug.Log($"{gameObject.name} 被选中");
        }

        public void OnTabDeselected()
        {
            // 页面被隐藏时触发：可以停止动画、清理内存
        }
    }
}