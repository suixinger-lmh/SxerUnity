using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sxer.Plugin.UISystem.Core;
using Sxer.Plugin.UISystem.Interfaces;
using Cysharp.Threading.Tasks;

namespace Sxer.Plugin.UISystem.Editor
{
    public class UIDashboard : EditorWindow
    {
        private UIConfig targetConfig;
        private int selectedTab = 0;
        private readonly string[] tabs = { "🛠️ 配置表管理", "👁️ 运行时监控", "🔍 场景分析与导出" };

        // 场景分析相关状态
        private List<UIPanel> scenePanels = new List<UIPanel>();
        private Vector2 sceneScrollPos;
        private Vector2 configScrollPos;
        private Vector2 monitorScrollPos;

        [MenuItem("GraceUI/控制面板 (Dashboard)", false, 1)]
        public static void OpenWindow()
        {
            var window = GetWindow<UIDashboard>("GraceUI Dashboard");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        private void OnEnable()
        {
            // 自动寻找配置表
            string[] guids = AssetDatabase.FindAssets("t:UIConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                targetConfig = AssetDatabase.LoadAssetAtPath<UIConfig>(path);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(30));
            EditorGUILayout.Space(10);

            switch (selectedTab)
            {
                case 0: DrawConfigTab(); break;
                case 1: DrawMonitorTab(); break;
                case 2: DrawSceneAnalyzerTab(); break;
            }
        }

        #region Tab 1: 配置管理
        private void DrawConfigTab()
        {
            GUILayout.Label("UI 预制体路径配置", EditorStyles.boldLabel);
            targetConfig = (UIConfig)EditorGUILayout.ObjectField("当前配置表 (UIConfig)", targetConfig, typeof(UIConfig), false);

            if (targetConfig == null)
            {
                EditorGUILayout.HelpBox("请先创建或指派一个 UIConfig 文件！(右键 Create -> GraceUI -> UI Config)", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            targetConfig.scanDirectory = EditorGUILayout.TextField("预制体导出/扫描根目录", targetConfig.scanDirectory);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(targetConfig);
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("🚀 一键扫描目录并更新配置表", GUILayout.Height(40)))
            {
                AutoCollectPrefabs();
            }

            EditorGUILayout.Space(10);
            GUILayout.Label($"当前已映射数量: {targetConfig.panelConfigs.Count}", EditorStyles.miniLabel);

            configScrollPos = EditorGUILayout.BeginScrollView(configScrollPos, "box");
            foreach (var item in targetConfig.panelConfigs)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.PanelClassName, EditorStyles.boldLabel, GUILayout.Width(200));
                EditorGUILayout.LabelField(item.PrefabPath, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
                GUILayout.Box(string.Empty, GUILayout.Height(1), GUILayout.ExpandWidth(true)); // 分割线
            }
            EditorGUILayout.EndScrollView();
        }

        private void AutoCollectPrefabs()
        {
            if (string.IsNullOrEmpty(targetConfig.scanDirectory)) return;

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { targetConfig.scanDirectory });
            List<UIPanelConfigItem> newList = new List<UIPanelConfigItem>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                UIPanel panelComponent = prefab.GetComponent<UIPanel>();
                if (panelComponent != null)
                {
                    string className = panelComponent.GetType().Name;
                    string resourcesPath = path.Replace(".prefab", "");
                    int resIndex = resourcesPath.IndexOf("Resources/");
                    if (resIndex >= 0) resourcesPath = resourcesPath.Substring(resIndex + 10);

                    newList.Add(new UIPanelConfigItem { PanelClassName = className, PrefabPath = resourcesPath });
                }
            }
            targetConfig.panelConfigs = newList;
            EditorUtility.SetDirty(targetConfig);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("成功", $"扫描完成！共找到 {newList.Count} 个 UI 面板。", "确定");
        }
        #endregion

        #region Tab 2: 运行时监控
        private void DrawMonitorTab()
        {
            GUILayout.Label("运行场景中的 UI 状态 (需在 Play 模式下)", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("请先运行游戏 (Play Mode) 以查看 UI 状态。", MessageType.Info);
                return;
            }

            if (UIManager.Instance == null)
            {
                EditorGUILayout.HelpBox("场景中未找到 UIManager.Instance ！", MessageType.Warning);
                return;
            }

            var cache = UIManager.Instance.PanelCache;
            if (cache.Count == 0)
            {
                EditorGUILayout.HelpBox("当前没有任何已加载的 UI 面板。", MessageType.Info);
                return;
            }

            monitorScrollPos = EditorGUILayout.BeginScrollView(monitorScrollPos, "box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("面板类名", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("当前状态", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label("操作", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            List<Type> keys = new List<Type>(cache.Keys);
            foreach (var type in keys)
            {
                var panel = cache[type];
                if (panel == null) continue;

                bool isActive = panel.gameObject.activeSelf;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(type.Name, GUILayout.Width(150));

                GUI.color = isActive ? Color.green : Color.gray;
                EditorGUILayout.LabelField(isActive ? "🟢 显示中 (Active)" : "⚪ 隐藏中 (Cached)", EditorStyles.boldLabel, GUILayout.Width(120));
                GUI.color = Color.white;

                if (isActive && GUILayout.Button("强制关闭", GUILayout.Width(80)))
                {
                    UIManager.Instance.ClosePanelAsync(type).Forget();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            Repaint();
        }
        #endregion

        #region Tab 3: 场景分析与一键导出 (New Feature!)
        private void DrawSceneAnalyzerTab()
        {
            GUILayout.Label("编辑器场景 UI 健康检查与导出", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("功能：扫描当前打开的 Scene 中所有的 UI 面板，检查是否缺少必备组件（如动画、关闭按钮），并支持一键将其保存为 Prefab 并登记到配置表中。", MessageType.Info);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("🔍 扫描当前场景", GUILayout.Height(30)))
            {
                // 兼容 Unity 老版本和新版本的写法，寻找场景中所有处于激活或未激活的 UIPanel
                scenePanels = Resources.FindObjectsOfTypeAll<UIPanel>()
                    .Where(p => !EditorUtility.IsPersistent(p.gameObject) // 排除项目目录里的Prefab，只找场景里的
                             && p.gameObject.hideFlags == HideFlags.None)
                    .ToList();
            }

            EditorGUILayout.Space(10);

            if (scenePanels.Count == 0)
            {
                EditorGUILayout.LabelField("场景中未发现 UIPanel 实例，请先点击扫描。", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            sceneScrollPos = EditorGUILayout.BeginScrollView(sceneScrollPos, "box");

            for (int i = scenePanels.Count - 1; i >= 0; i--)
            {
                UIPanel panel = scenePanels[i];
                if (panel == null)
                {
                    scenePanels.RemoveAt(i);
                    continue;
                }

                DrawPanelHealthCheckUI(panel);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawPanelHealthCheckUI(UIPanel panel)
        {
            Type panelType = panel.GetType();
            GameObject go = panel.gameObject;

            EditorGUILayout.BeginVertical("window"); // 使用 window 样式产生明显卡片效果

            // 1. 标题行
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"📦 {go.name} ({panelType.Name})", EditorStyles.boldLabel);
            if (GUILayout.Button("在场景中选中", GUILayout.Width(100)))
            {
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // 2. 依赖/关系检查 (Health Check)
            CheckAndDrawComponentStatus<IUITransitionAnimation>(go, "过渡动画 (Transition)");
            CheckAndDrawComponentStatus<IUILoopAnimation>(go, "循环动画 (Loop)");
            CheckAndDrawComponentStatus<IUIFeedbackAnimation>(go, "反馈动画 (Feedback)");

            // 检查受保护的 btnClose 按钮是否赋值（使用反射/序列化对象突破访问限制）
            SerializedObject so = new SerializedObject(panel);
            SerializedProperty btnProp = so.FindProperty("btnClose");
            bool hasCloseBtn = btnProp != null && btnProp.objectReferenceValue != null;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("❌", GUILayout.Width(20)); // 作为图标对齐
            GUI.color = hasCloseBtn ? Color.white : new Color(1f, 0.6f, 0.2f); // 未赋值显示橙色警告
            GUILayout.Label($"退出按钮 (btnClose): {(hasCloseBtn ? "已绑定" : "未绑定(可能导致无法通用关闭)")}");
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 3. 导出 Prefab 功能
            if (targetConfig == null || string.IsNullOrEmpty(targetConfig.scanDirectory))
            {
                EditorGUILayout.HelpBox("导出失败：请在配置表页面设置好 Scan Directory！", MessageType.Error);
            }
            else
            {
                // 检查该节点是否本身就是Prefab实例
                bool isPrefabInstance = PrefabUtility.IsPartOfAnyPrefab(go);
                string exportActionName = isPrefabInstance ? "🔄 应用覆盖 Prefab 并注册" : "💾 导出为新 Prefab 并注册";

                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                if (GUILayout.Button(exportActionName, GUILayout.Height(30)))
                {
                    ExportToPrefabAndRegister(panel);
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void CheckAndDrawComponentStatus<T>(GameObject go, string name) where T : class
        {
            var comp = go.GetComponent<T>();
            bool hasComp = comp != null;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(hasComp ? "✅" : "⚠️", GUILayout.Width(20));
            GUI.color = hasComp ? Color.white : Color.gray;
            string compName = hasComp ? comp.GetType().Name : "未挂载";
            GUILayout.Label($"{name} : {compName}");
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // 核心逻辑：一键导出并更新配置
        private void ExportToPrefabAndRegister(UIPanel panel)
        {
            string dirPath = targetConfig.scanDirectory;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string className = panel.GetType().Name;
            string fullPath = $"{dirPath}/{className}.prefab";

            // 1. 保存/覆盖 Prefab，并自动连接场景中的对象（使场景里的物体变成这个Prefab的实例）
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(panel.gameObject, fullPath, InteractionMode.UserAction);
            if (savedPrefab != null)
            {
                Debug.Log($"[GraceUI] 成功导出预制体: {fullPath}");

                // 2. 自动注册到 UIConfig
                string resourcesPath = fullPath.Replace(".prefab", "");
                int resIndex = resourcesPath.IndexOf("Resources/");
                if (resIndex >= 0) resourcesPath = resourcesPath.Substring(resIndex + 10);

                // 检查是否已经存在
                var existingItem = targetConfig.panelConfigs.Find(x => x.PanelClassName == className);
                if (existingItem != null)
                {
                    existingItem.PrefabPath = resourcesPath;
                }
                else
                {
                    targetConfig.panelConfigs.Add(new UIPanelConfigItem { PanelClassName = className, PrefabPath = resourcesPath });
                }

                EditorUtility.SetDirty(targetConfig);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("导出成功", $"面板 {className} 已成功保存为 Prefab 并注册到配置表中！\n路径: {fullPath}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("导出失败", "Prefab 保存失败，请检查路径是否合法。", "确定");
            }
        }
        #endregion
    }
}