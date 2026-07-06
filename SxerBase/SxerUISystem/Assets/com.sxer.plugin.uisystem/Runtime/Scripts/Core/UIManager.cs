using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sxer.Plugin.UISystem.Interfaces;
using UnityEngine;

namespace Sxer.Plugin.UISystem.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI 配置表")]
        public UIConfig uiConfig; // <--- 在面板中拖拽赋值

        public static UIManager Instance { get; private set; }

        private IUIResourceLoader resourceLoader;
        private Dictionary<Type, UIPanel> panelCache = new Dictionary<Type, UIPanel>();
        public IReadOnlyDictionary<Type, UIPanel> PanelCache => panelCache;

        [Header("UI 层级节点")]
        public Transform NormalLayer;
        public Transform PopUpLayer;
        public Transform TopLayer;

        public void Initialize(IUIResourceLoader loader)
        {
            Instance = this;
            this.resourceLoader = loader;
        }

        #region 内部通用逻辑

        // 提取复用的加载/获取逻辑
        private async UniTask<T> GetOrLoadPanelAsync<T>() where T : UIPanel
        {
            Type type = typeof(T);
            if (!panelCache.TryGetValue(type, out UIPanel basePanel))
            {
                // 1. 通过 UIConfig 获取真实路径
                string path = uiConfig.GetPrefabPath(type.Name);
                if (string.IsNullOrEmpty(path)) return null;

                // 2. 加载实例化 (这里加载器不需要再拼写前缀了，直接用 path)
                GameObject go = await resourceLoader.InstantiateAsync(path, NormalLayer);
                T panel = go.GetComponent<T>();
                panelCache[type] = panel;
                panel.OnInit();
                return panel;
            }

            T cachedPanel = basePanel as T;
            cachedPanel.gameObject.SetActive(true);
            return cachedPanel;
        }

        // 提取复用的动画和交互控制逻辑
        private async UniTask PlayEnterAnimation(UIPanel panel)
        {
            panel.CanvasGroup.interactable = false; // 动画期间防误触

            var transition = panel.GetComponent<IUITransitionAnimation>();
            if (transition != null)
            {
                await transition.PlayEnterAsync();
            }

            panel.CanvasGroup.interactable = true; // 动画结束恢复交互
        }

        #endregion

        #region 公开 API

        /// <summary>
        /// 打开无参数面板
        /// </summary>
        public async UniTask<T> OpenPanelAsync<T>() where T : UIPanel
        {
            T panel = await GetOrLoadPanelAsync<T>();
            panel.OnOpen();
            await PlayEnterAnimation(panel);
            return panel;
        }

        /// <summary>
        /// 打开带强类型参数的面板
        /// </summary>
        public async UniTask<TPanel> OpenPanelAsync<TPanel, TData>(TData data) where TPanel : UIPanel<TData>
        {
            TPanel panel = await GetOrLoadPanelAsync<TPanel>();
            panel.OnOpen(data); // 传入强类型数据刷新UI
            await PlayEnterAnimation(panel);
            return panel;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public async UniTask ClosePanelAsync(Type type)
        {
            if (panelCache.TryGetValue(type, out UIPanel panel) && panel.gameObject.activeSelf)
            {
                panel.CanvasGroup.interactable = false;

                var transition = panel.GetComponent<IUITransitionAnimation>();
                if (transition != null)
                {
                    await transition.PlayExitAsync();
                }

                panel.OnClose();
                panel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 泛型快捷关闭
        /// </summary>
        public UniTask ClosePanelAsync<T>() where T : UIPanel
        {
            return ClosePanelAsync(typeof(T));
        }

        #endregion
    }
}