using UnityEngine;
using UnityEditor;
using Sxer.Plugin.ProcessManagement;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Sxer.Plugin.ProcessManagement.Editor
{
    public class ProcessManagerConsole : EditorWindow
    {
        // 缓存所有可用Process类型
        private static List<Type> _cachedProcessTypes = new List<Type>();

        // 脚本编译完成、编辑器启动自动刷新缓存
        [InitializeOnLoadMethod]
        private static void RefreshProcessTypeCache()
        {
            _cachedProcessTypes.Clear();
            var types = TypeCache.GetTypesDerivedFrom<ProcessBase>()
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .ToList();
            _cachedProcessTypes = types;
        }

        #region 新建流程 - 编辑缓存参数
        private int _selectTypeIndex;
        private bool _editAutoExecute;
        private int _editPriority;
        private string _editCustomId;
        private string _editDescription;
        private string[] TypeNames => _cachedProcessTypes.Select(t => t.Name).ToArray();
        #endregion

        private Vector2 scrollPos;

        [MenuItem("Tools/Sxer Process/全局流程控制台 (Manager Console)")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProcessManagerConsole>("流程控制台");
            window.minSize = new Vector2(450, 550);
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying) Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("📊 全局流程监控看板", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            // 【顶部：新建流程面板（优化布局+参数编辑）】
            DrawCreateProcessPanel();

            EditorGUILayout.Space(16);
            DrawRuntimeData();

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField("🔍 场景全量流程实体扫描", EditorStyles.boldLabel);
            DrawSceneProcesses();
        }

        #region 优化后的创建面板（带参数编辑）
        private void DrawCreateProcessPanel()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("➕ 新建流程实体", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (_cachedProcessTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("未扫描到任何 ProcessBase 实现类，无法创建", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            // 1. 流程类型下拉选择
            EditorGUI.BeginChangeCheck();
            _selectTypeIndex = EditorGUILayout.Popup("目标流程类型", _selectTypeIndex, TypeNames);
            // 切换类型时重置所有编辑参数为默认值
            if (EditorGUI.EndChangeCheck())
            {
                ResetCreateParamDefault();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("参数预设（创建时自动赋值）", EditorStyles.miniBoldLabel);
            EditorGUILayout.Space(2);

            // 2. 参数编辑分组
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;

            _editAutoExecute = EditorGUILayout.Toggle("自动执行 autoExecute", _editAutoExecute);
            _editPriority = EditorGUILayout.IntField("执行优先级 priority", _editPriority);
            _editCustomId = EditorGUILayout.TextField("自定义ID processId", _editCustomId);

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("描述信息 Description", EditorStyles.miniLabel);
            _editDescription = EditorGUILayout.TextArea(_editDescription, GUILayout.Height(45));

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            // 创建按钮
            if (GUILayout.Button("创建到场景（应用上方参数）", GUILayout.Height(26)))
            {
                Type targetType = _cachedProcessTypes[_selectTypeIndex];
                CreateProcessInstance(targetType);
                // 创建完成重置输入，方便连续新建
                ResetCreateParamDefault();
            }

            EditorGUILayout.Space(3);
            EditorGUILayout.HelpBox("选中Hierarchy物体则作为子物体生成；无选中则创建在场景根节点", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        /// <summary> 重置新建参数为默认值 </summary>
        private void ResetCreateParamDefault()
        {
            _editAutoExecute = false;
            _editPriority = 0;
            string typeName = TypeNames[_selectTypeIndex];
            _editCustomId = $"{typeName}_";
            _editDescription = "";
        }

        /// <summary> 根据选中类型 + 面板预设参数生成流程物体并赋值 </summary>
        private void CreateProcessInstance(Type targetType)
        {
            GameObject parent = Selection.activeObject as GameObject;
            GameObject go = new GameObject(targetType.Name);

            if (parent != null)
            {
                GameObjectUtility.SetParentAndAlign(go, parent);
            }

            if (go.AddComponent(targetType) is ProcessBase comp)
            {
                // 应用面板填写的所有参数
                comp.autoExecute = _editAutoExecute;
                comp.priority = _editPriority;

                // 自定义ID非空则使用，否则自动生成唯一ID
                if (!string.IsNullOrWhiteSpace(_editCustomId))
                    comp.processId = _editCustomId;
                else
                    comp.processId = $"{targetType.Name}_{go.GetInstanceID()}";

                // 如果你ProcessBase有Description字段，直接赋值
                var descField = comp.GetType().GetField("description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                descField?.SetValue(comp, _editDescription);
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {targetType.Name} Process");
            Selection.activeObject = go;
            EditorGUIUtility.PingObject(go);
            Repaint();
        }
        #endregion

        private void DrawRuntimeData()
        {
            EditorGUILayout.BeginVertical("box");
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入运行模式(Play Mode)后显示动态数据。", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var manager = FindAnyObjectByType<ProcessManager>();
            if (manager == null)
            {
                EditorGUILayout.HelpBox("场景中未找到 ProcessManager 实例！", MessageType.Error);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField($"当前运行模式: {manager.CurrentMode}");

            var current = manager.GetCurrentProcess();
            string currentName = current != null ? $"{current.processId} ({current.GetType().Name})" : "空闲 (None)";
            EditorGUILayout.LabelField("▶ 当前主运行流程:", currentName, EditorStyles.boldLabel);

            int queueCount = GetPrivateListCount(manager, "autoExecutionQueue");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"自动执行等待队列数: {queueCount}");
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSceneProcesses()
        {
            ProcessBase[] allProcesses = FindObjectsByType<ProcessBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (allProcesses.Length == 0)
            {
                EditorGUILayout.HelpBox("场景中没有任何 ProcessBase 实例。", MessageType.Warning);
                return;
            }

            // 核心：按优先级 降序排序，数值大的排在上方
            var sortedList = allProcesses.OrderByDescending(p => p.priority).ToList();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "helpbox");
            foreach (var p in sortedList)
            {
                EditorGUILayout.BeginVertical("Box");

                // 第一行：状态标签 + 物体名 + ProcessId + 定位按钮
                EditorGUILayout.BeginHorizontal();
                GUI.color = GetStateColor(p.State);
                EditorGUILayout.LabelField($"[{p.State}]", GUILayout.Width(75));
                GUI.color = Color.white;

                EditorGUILayout.LabelField($"物体：{p.gameObject.name}", EditorStyles.miniLabel);
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField($"ID：{p.processId}", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("定位", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = p.gameObject;
                    EditorGUIUtility.PingObject(p.gameObject);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2);

                // 第二行：Auto执行标记 + 优先级
                EditorGUILayout.BeginHorizontal();
                string autoTag = p.autoExecute ? "【自动执行】" : "手动触发";
                EditorGUILayout.LabelField($"{autoTag} | 优先级：{p.priority}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // 第三行：描述文本（非空才显示）
                string desc = GetProcessDescription(p);
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    EditorGUILayout.Space(2);
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField($"描述：{desc}", EditorStyles.miniLabel);
                    GUI.color = Color.white;
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("刷新扫描", GUILayout.Height(25)))
            {
                Repaint();
            }
        }

        // 辅助方法：反射读取描述字段
        private string GetProcessDescription(ProcessBase process)
        {
            var field = process.GetType().GetField("description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return string.Empty;
            var value = field.GetValue(field.DeclaringType.IsInstanceOfType(process) ? process : null);
            return value?.ToString() ?? string.Empty;
        }

        private Color GetStateColor(ProcessState state)
        {
            switch (state)
            {
                case ProcessState.Running: return Color.green;
                case ProcessState.Paused: return new Color(1f, 0.8f, 0f);
                case ProcessState.Completed: return Color.cyan;
                default: return Color.gray;
            }
        }

        private int GetPrivateListCount(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var val = field.GetValue(target);
                if (val != null)
                {
                    var countProp = val.GetType().GetProperty("Count");
                    if (countProp != null) return (int)countProp.GetValue(val);
                }
            }
            return 0;
        }
    }
}