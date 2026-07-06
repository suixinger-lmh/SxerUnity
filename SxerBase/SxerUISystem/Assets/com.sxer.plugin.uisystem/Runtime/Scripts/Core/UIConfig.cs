using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sxer.Plugin.UISystem.Core
{
    [Serializable]
    public class UIPanelConfigItem
    {
        public string PanelClassName; // 面板脚本的类名 (例如: SettingsPanel)
        public string PrefabPath;     // 加载路径 (例如: UI/Panels/SettingsPanel)
    }

    [CreateAssetMenu(fileName = "UIConfig", menuName = "GraceUI/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Header("UI 预制体扫描目录 (Editor使用)")]
        public string scanDirectory = "Assets/Resources/UI";

        [Header("映射数据")]
        public List<UIPanelConfigItem> panelConfigs = new List<UIPanelConfigItem>();

        // 运行时缓存字典，加速查找
        private Dictionary<string, string> runtimeCache;

        /// <summary>
        /// 运行时根据类名获取路径
        /// </summary>
        public string GetPrefabPath(string className)
        {
            if (runtimeCache == null)
            {
                runtimeCache = new Dictionary<string, string>();
                foreach (var item in panelConfigs)
                {
                    runtimeCache[item.PanelClassName] = item.PrefabPath;
                }
            }

            if (runtimeCache.TryGetValue(className, out string path))
            {
                return path;
            }

            Debug.LogError($"[UIConfig] 找不到面板 {className} 的路径配置，请检查配置表！");
            return string.Empty;
        }
    }
}