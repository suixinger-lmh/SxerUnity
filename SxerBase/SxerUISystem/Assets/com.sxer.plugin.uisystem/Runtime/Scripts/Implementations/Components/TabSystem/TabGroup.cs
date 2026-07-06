using Sxer.Plugin.UISystem.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sxer.Plugin.UISystem
{
    public class TabGroup : MonoBehaviour, ITabGroup
    {
        [Header("标签按钮 (Toggles)")]
        public List<Toggle> tabToggles;

        [Header("标签内容页 (需实现 ITabPage)")]
        public List<GameObject> tabPages;

        private ITabPage[] cachedPages;
        private int currentIndex = -1;

        private void Awake()
        {
            cachedPages = new ITabPage[tabPages.Count];
            for (int i = 0; i < tabPages.Count; i++)
            {
                cachedPages[i] = tabPages[i].GetComponent<ITabPage>();

                int index = i; // 闭包陷阱处理
                tabToggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) SwitchTab(index);
                });
            }
        }

        public void SwitchTab(int index)
        {
            if (index == currentIndex || index < 0 || index >= cachedPages.Length) return;

            // 1. 取消上一个
            if (currentIndex >= 0 && cachedPages[currentIndex] != null)
            {
                cachedPages[currentIndex].OnTabDeselected();
                tabPages[currentIndex].SetActive(false);
            }

            // 2. 选中新的
            currentIndex = index;
            tabPages[currentIndex].SetActive(true);
            cachedPages[currentIndex]?.OnTabSelected();
        }
    }
}