#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Sxer.Plugin.EventSystem.Cmd.Editor
{
    public class CommandEventWindow : EditorWindow
    {
        private Dictionary<Type, List<Type>> _commandTree = new Dictionary<Type, List<Type>>();
        private Vector2 _scrollPos;

        [MenuItem("Sxer/EventSystem/指令事件面板 (Command Center)")]
        public static void ShowWindow()
        {
            GetWindow<CommandEventWindow>("指令总览").RefreshData();
        }

        private void RefreshData()
        {
            _commandTree.Clear();
            var categories = CommandReflectionUtil.GetCategories();
            foreach (var cat in categories)
            {
                _commandTree.Add(cat, CommandReflectionUtil.GetConcreteCommands(cat));
            }
        }

        private void OnGUI()
        {
            if (GUILayout.Button("刷新数据", GUILayout.Height(30))) RefreshData();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var kvp in _commandTree)
            {
                EditorGUILayout.BeginVertical("box");

                // 分类头部
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"分类: {kvp.Key.Name}", EditorStyles.boldLabel);
                if (GUILayout.Button("在场景创建该类型的 CMDManager", GUILayout.Width(250)))
                {
                    CreateManagerInScene(kvp.Key);
                }
                EditorGUILayout.EndHorizontal();

                // 具体的指令列表
                EditorGUI.indentLevel++;
                if (kvp.Value.Count == 0)
                {
                    GUILayout.Label("  (无具体实现)", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var cmdType in kvp.Value)
                    {
                        var dummy = (CommandHandler)Activator.CreateInstance(cmdType);
                        GUILayout.Label($"[{dummy.CommandId}] - {cmdType.Name}  ({dummy.CmdDesc})");
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }

        private void CreateManagerInScene(Type categoryType)
        {
            GameObject go = new GameObject($"CMDManager_{categoryType.Name}");
            var manager = go.AddComponent<CMDManager>();
            manager.managedCategoryTypeName = categoryType.FullName;
            Selection.activeGameObject = go;
            Debug.Log($"已在场景中创建 {categoryType.Name} 的指令管理器！");
        }
    }
}
#endif