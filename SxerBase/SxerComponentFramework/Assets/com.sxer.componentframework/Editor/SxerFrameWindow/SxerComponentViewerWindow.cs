using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Sxer.Frame.Editor
{
    public class SxerComponentViewerWindow : EditorWindow
    {
        // 内部数据结构，适配新版特性
        private class ComponentInfo
        {
            public Type type;
            public string className;
            public string description;
            public ComponentLifeType lifeType; // 区分单例或动态
            public string initType;
            public int priority;
            public bool isAbstract;
            public int sceneCount;
        }

        private List<ComponentInfo> _componentList = new List<ComponentInfo>();
        private Vector2 _scrollPos;

        // UI 样式缓存
        private GUIStyle _headerStyle;
        private GUIStyle _itemBgStyle;
        private GUIStyle _countStyleExist;
        private GUIStyle _countStyleMissing;
        private GUIStyle _categoryTitleStyle;

        [MenuItem("Sxer/Frame/框架组件管理器", false, 110)]
        public static void OpenWindow()
        {
            var window = GetWindow<SxerComponentViewerWindow>("Sxer 框架管线");
            window.minSize = new Vector2(900, 600);
            window.RefreshComponentList();
        }

        private void OnEnable()
        {
            RefreshComponentList();
            EditorApplication.hierarchyChanged += RefreshSceneCount; // 场景变化时自动刷新数量
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= RefreshSceneCount;
        }

        private void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, padding = new RectOffset(5, 5, 5, 5) };

                _itemBgStyle = new GUIStyle(GUI.skin.box) { margin = new RectOffset(5, 5, 2, 2), padding = new RectOffset(5, 5, 4, 4) };

                _countStyleExist = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.2f, 0.8f, 0.2f) } }; // 绿色
                _countStyleMissing = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } }; // 灰色

                _categoryTitleStyle = new GUIStyle(EditorStyles.whiteLargeLabel)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(0.4f, 0.7f, 1f) } // 浅蓝色
                };
            }
        }

        #region 核心数据收集
        public void RefreshComponentList()
        {
            _componentList.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // 过滤出系统库，加快反射速度
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("mscorlib")) continue;

                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsGenericType && t.IsSubclassOf(typeof(SxerComponentBase)));

                    foreach (var type in types)
                    {
                        var info = new ComponentInfo
                        {
                            type = type,
                            className = type.Name,
                            isAbstract = type.IsAbstract
                        };

                        // 尝试获取我们新设计的 Attribute
                        var attr = type.GetCustomAttribute<SxerComponentAttribute>(false);
                        if (attr != null)
                        {
                            info.lifeType = attr.LifeType;
                            info.initType = attr.InitType.ToString();
                            info.priority = attr.InitPriority;
                            info.description = attr.Description;
                        }
                        else
                        {
                            // 如果没有打标签，通过基类名称推断（兜底逻辑）
                            info.lifeType = type.BaseType != null && type.BaseType.Name.Contains("Singleton")
                                ? ComponentLifeType.GlobalSingleton
                                : ComponentLifeType.DynamicInstance;

                            info.initType = "Sync (Default)";
                            info.priority = 100;
                            info.description = "未标记描述";
                        }

                        info.sceneCount = FindObjectsOfType(type, true).Length;
                        _componentList.Add(info);
                    }
                }
                catch { }
            }

            // 按优先级排序
            _componentList = _componentList.OrderBy(c => c.priority).ToList();
        }

        private void RefreshSceneCount()
        {
            foreach (var info in _componentList)
            {
                info.sceneCount = FindObjectsOfType(info.type, true).Length;
            }
            Repaint(); // 强制重绘 UI
        }
        #endregion

        #region UI 绘制逻辑
        private void OnGUI()
        {
            InitStyles();
            DrawTopToolbar();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // 分离两类组件
            var singletons = _componentList.Where(c => c.lifeType == ComponentLifeType.GlobalSingleton).ToList();
            var dynamics = _componentList.Where(c => c.lifeType == ComponentLifeType.DynamicInstance).ToList();

            DrawCategory("🌐 全局单例组件 (Global Singletons)", singletons, true);
            EditorGUILayout.Space(10);
            DrawCategory("⚔️ 动态实例组件 (Dynamic Instances)", dynamics, false);

            EditorGUILayout.EndScrollView();
        }

        private void DrawTopToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"已搜集组件总数: {_componentList.Count}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("一键补齐所有单例", EditorStyles.toolbarButton))
                BatchCreateMissingSingletons();

            if (GUILayout.Button("清理所有框架组件", EditorStyles.toolbarButton))
                ClearAllComponents();

            if (GUILayout.Button("强制刷新", EditorStyles.toolbarButton))
                RefreshComponentList();
            if (GUILayout.Button("一键生成预设", EditorStyles.toolbarButton))
                GenerateFramePrefab();

            GUILayout.EndHorizontal();
        }

        private void DrawCategory(string title, List<ComponentInfo> list, bool isSingleton)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, _categoryTitleStyle, GUILayout.Height(25));
            GUILayout.EndHorizontal();

            // 绘制表头
            GUILayout.BeginHorizontal();
            GUILayout.Label("类名 (描述)", _headerStyle, GUILayout.Width(250));
            GUILayout.Label("Init", _headerStyle, GUILayout.Width(60));
            GUILayout.Label("优先", _headerStyle, GUILayout.Width(40));
            GUILayout.Label("场景数", _headerStyle, GUILayout.Width(50));
            GUILayout.Label("操作面板", _headerStyle, GUILayout.Width(350));
            GUILayout.EndHorizontal();

            if (list.Count == 0)
            {
                EditorGUILayout.HelpBox("未扫描到此类组件", MessageType.Info);
                return;
            }

            foreach (var comp in list)
            {
                GUILayout.BeginHorizontal(_itemBgStyle);

                // 类名与描述
                string displayName = string.IsNullOrEmpty(comp.description) ? comp.className : $"{comp.className}\n<size=9><color=gray>{comp.description}</color></size>";
                GUILayout.Label(new GUIContent(displayName, comp.description), new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(250));

                GUILayout.Label(comp.initType, GUILayout.Width(60));
                GUILayout.Label(comp.priority.ToString(), GUILayout.Width(40));

                // 数量标红/标绿
                GUIStyle countStyle = comp.sceneCount > 0 ? _countStyleExist : _countStyleMissing;
                GUILayout.Label(comp.sceneCount.ToString(), countStyle, GUILayout.Width(50));

                if (comp.isAbstract)
                {
                    GUI.enabled = false;
                    GUILayout.Button("抽象类不可挂载", GUILayout.Width(335));
                    GUI.enabled = true;
                }
                else
                {
                    DrawOperationButtons(comp, isSingleton);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawOperationButtons(ComponentInfo comp, bool isSingleton)
        {
            // 添加/挂载
            if (GUILayout.Button(isSingleton ? "挂载到框架" : "在场景生成", GUILayout.Width(80)))
            {
                if (isSingleton) AddSingletonToFrame(comp.type);
                else AddDynamicToScene(comp.type);
                RefreshSceneCount();
            }

            // 如果场景中存在，才允许卸载和定位
            GUI.enabled = comp.sceneCount > 0;

            if (GUILayout.Button("卸载", GUILayout.Width(50)))
            { RemoveComponentFromScene(comp.type); RefreshSceneCount(); }

            if (GUILayout.Button("定位", GUILayout.Width(50)))
            { SelectComponentsInScene(comp.type); }

            GUI.enabled = true;

            // 定位脚本
            if (GUILayout.Button("脚本", GUILayout.Width(50)))
            { PingScript(comp.type); }

            // 运行时状态 (仅在Play模式可用)
            GUI.enabled = Application.isPlaying && comp.sceneCount > 0;
            if (GUILayout.Button("运行状态", GUILayout.Width(90)))
            { ShowRuntimeState(comp.type); }
            GUI.enabled = true;
        }
        #endregion

        #region 操作逻辑实现
        private void AddSingletonToFrame(Type componentType)
        {
            var frame = FindObjectOfType<SxerFrame>(true);
            if (frame == null)
            {
                GameObject frameGo = new GameObject("SxerFrame");
                frame = frameGo.AddComponent<SxerFrame>();
                Undo.RegisterCreatedObjectUndo(frameGo, "Create SxerFrame");
            }

            var exists = frame.GetComponentsInChildren(componentType, true);
            if (exists.Length > 0)
            {
                EditorUtility.DisplayDialog("提示", "该单例组件已存在于框架中！", "确定");
                EditorGUIUtility.PingObject(exists[0].gameObject);
                return;
            }

            GameObject go = new GameObject($"[Singleton]-{componentType.Name}");
            go.transform.SetParent(frame.transform, false);
            go.AddComponent(componentType);
            Undo.RegisterCreatedObjectUndo(go, "Add Singleton");

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void AddDynamicToScene(Type componentType)
        {
            GameObject go = new GameObject($"[Dynamic]-{componentType.Name}");
            go.AddComponent(componentType);
            Undo.RegisterCreatedObjectUndo(go, "Create Dynamic Component");

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void RemoveComponentFromScene(Type componentType)
        {
            var comps = FindObjectsOfType(componentType, true) as MonoBehaviour[];
            if (comps.Length == 0) return;

            foreach (var c in comps)
            {
                Undo.DestroyObjectImmediate(c.gameObject);
            }
        }

        private void BatchCreateMissingSingletons()
        {
            var singletons = _componentList.Where(c => c.lifeType == ComponentLifeType.GlobalSingleton && !c.isAbstract).ToList();
            int addCount = 0;
            foreach (var s in singletons)
            {
                if (s.sceneCount == 0)
                {
                    AddSingletonToFrame(s.type);
                    addCount++;
                }
            }
            RefreshSceneCount();
            EditorUtility.DisplayDialog("完成", $"成功补齐 {addCount} 个缺失的单例组件！", "确定");
        }

        private void SelectComponentsInScene(Type componentType)
        {
            var comps = FindObjectsOfType(componentType, true) as MonoBehaviour[];
            if (comps.Length > 0) Selection.objects = comps.Select(c => c.gameObject).ToArray();
        }

        private void PingScript(Type type)
        {
            var guides = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var g in guides)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    EditorGUIUtility.PingObject(script);
                    Selection.activeObject = script;
                    return;
                }
            }
        }

        private void ShowRuntimeState(Type type)
        {
            if (!Application.isPlaying) return;

            var comps = FindObjectsOfType(type, true) as SxerComponentBase[];
            string info = $"【{type.Name}】 当前运行时状态:\n\n";
            foreach (var c in comps)
            {
                info += $"挂载对象: {c.gameObject.name}\n" +
                        $"框架调度类型: {c.LifeType}\n" +
                        $"当前生命周期: {c.ComponentState}\n" +
                        $"初始化结果: {(c.ComponentState == ComponentState.Inited ? c.InitResult.ToString() : "尚未完成")}\n\n";
            }
            EditorUtility.DisplayDialog("组件实时状态监控", info, "关闭");
        }

        private void ClearAllComponents()
        {
            if (!EditorUtility.DisplayDialog("高危操作", "确定要清空场景中【所有】的 SxerFrame 及其子组件吗？", "确定清空", "取消"))
                return;

            var frame = FindObjectOfType<SxerFrame>(true);
            if (frame != null)
            {
                Undo.DestroyObjectImmediate(frame.gameObject);
            }

            RefreshSceneCount();
        }

        private void GenerateFramePrefab()
        {
            // 1. 找到场景中的SxerFrame
            SxerFrame frame = FindObjectOfType<SxerFrame>(true);
            if (frame == null)
            {
                EditorUtility.DisplayDialog("提示", "当前场景未找到 SxerFrame 对象，请先创建！", "确定");
                return;
            }

            // 2. 定义保存路径
            string folderPath = "Assets/SxerFrame/Resources/SxerFramePrefab";
            string prefabPath = $"{folderPath}/SxerFrame.prefab";

            // 3. 目录不存在则创建
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // 4. 生成/替换预制体
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(frame.gameObject, prefabPath);

            if (prefab != null)
            {
                EditorUtility.DisplayDialog("成功", $"Frame 预设已生成/替换：\n{prefabPath}", "确定");
                EditorGUIUtility.PingObject(prefab); // 定位到预设
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "预制体生成失败！", "确定");
            }
        }
        #endregion
    }
}