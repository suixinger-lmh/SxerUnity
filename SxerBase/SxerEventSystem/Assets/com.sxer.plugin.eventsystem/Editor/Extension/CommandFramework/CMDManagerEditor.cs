#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sxer.Plugin.EventSystem.Cmd.Editor
{
    [CustomEditor(typeof(CMDManager))]
    public class CMDManagerEditor : UnityEditor.Editor
    {
        private CMDManager _target;
        private ReorderableList _listUI;

        private List<Type> _allCategories;
        private string[] _categoryOptions;

        // 当前选中的分类相关的指令数据
        private List<Type> _concreteTypes = new List<Type>();
        private string[] _cmdIds;
        private string[] _cmdDropdownOptions;

        private void OnEnable()
        {
            _target = (CMDManager)target;

            // 1. 获取所有大分类
            _allCategories = CommandReflectionUtil.GetCategories();
            _categoryOptions = _allCategories.Select(t => t.Name).ToArray();

            // 检查当前保存的分类是否依然合法（是否存在于反射获取到的分类列表中）
            bool isValidCategory = _allCategories.Any(t => t.FullName == _target.managedCategoryTypeName);

            // 如果当前没有设置分类，或者设置的分类已经失效（如由于 namespace 改变导致找不到），默认自愈设为第一个合法分类
            if ((string.IsNullOrEmpty(_target.managedCategoryTypeName) || !isValidCategory) && _allCategories.Count > 0)
            {
                _target.managedCategoryTypeName = _allCategories[0].FullName;
                EditorUtility.SetDirty(_target);
            }

            // 2. 刷新当前分类的具体指令
            RefreshConcreteCommands();

            // 3. 绘制 ReorderableList
            _listUI = new ReorderableList(serializedObject, serializedObject.FindProperty("handlers"), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "管理的指令列表 (支持拖拽排序)"),
                drawElementCallback = DrawElement,
                onAddCallback = OnAddItem,
                onRemoveCallback = OnRemoveItem
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();

            if (_allCategories.Count == 0)
            {
                EditorGUILayout.HelpBox("项目中未找到任何继承自 CommandHandler 的分类抽象类（例如 JsCommandHandler）。", MessageType.Error);
                return;
            }

            // === 1. 分类选择器 ===
            int currentIndex = _allCategories.FindIndex(t => t.FullName == _target.managedCategoryTypeName);
            if (currentIndex == -1) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("当前管理类型:", currentIndex, _categoryOptions);
            if (newIndex != currentIndex && _allCategories.Count > 0)
            {
                if (EditorUtility.DisplayDialog("切换管理类型", "切换类型将清空当前绑定的所有指令，是否继续？", "确定", "取消"))
                {
                    Undo.RecordObject(_target, "Change Category");
                    _target.managedCategoryTypeName = _allCategories[newIndex].FullName;
                    _target.handlers.Clear();
                    RefreshConcreteCommands();
                    EditorUtility.SetDirty(_target);
                    serializedObject.Update();
                }
            }

            EditorGUILayout.Space();

            // === 2. 指令列表 ===
            if (_concreteTypes.Count == 0)
            {
                EditorGUILayout.HelpBox($"当前分类 ({_target.managedCategoryTypeName}) 下没有找到任何具体的指令实现！\n请确保具体指令类已写好，且未处于错误的程序集中。", MessageType.Warning);
            }
            else
            {
                _listUI.DoLayoutList();
            }

            serializedObject.ApplyModifiedProperties();

            // === 3. 一键功能 ===
            if (_concreteTypes.Count > 0 && GUILayout.Button("一键绑定该类型下的所有指令", GUILayout.Height(30)))
            {
                Undo.RecordObject(_target, "Bind All");
                _target.handlers.Clear();
                foreach (var type in _concreteTypes)
                {
                    _target.handlers.Add((CommandHandler)Activator.CreateInstance(type));
                }
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }
        }

        private void RefreshConcreteCommands()
        {
            var catType = _allCategories.Find(t => t.FullName == _target.managedCategoryTypeName);
            if (catType != null)
            {
                _concreteTypes = CommandReflectionUtil.GetConcreteCommands(catType);
                _cmdIds = new string[_concreteTypes.Count];
                _cmdDropdownOptions = new string[_concreteTypes.Count];

                for (int i = 0; i < _concreteTypes.Count; i++)
                {
                    var dummy = (CommandHandler)Activator.CreateInstance(_concreteTypes[i]);
                    _cmdIds[i] = dummy.CommandId;
                    _cmdDropdownOptions[i] = $"[{dummy.CommandId}] {_concreteTypes[i].Name}";
                }
            }
            else
            {
                _concreteTypes.Clear();
            }
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= _target.handlers.Count) return;
            var handler = _target.handlers[index];
            rect.y += 2;

            // 增强的 Null 保护与调试可视化
            if (handler == null)
            {
                if (_concreteTypes.Count > 0)
                {
                    handler = (CommandHandler)Activator.CreateInstance(_concreteTypes[0]);
                    _target.handlers[index] = handler;
                    EditorUtility.SetDirty(_target);
                    serializedObject.Update();
                }
                else
                {
                    // 若无具体类型，在面板上渲染红色错误提示，方便您排查
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        "错误: 无法实例化！未找到该分类下的任何具体指令类型。", MessageType.Error);
                    return;
                }
            }

            int selectedCmdIndex = Array.IndexOf(_cmdIds, handler.CommandId);
            if (selectedCmdIndex == -1) selectedCmdIndex = 0;

            float popupWidth = 140f, labelWidth = 40f;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), "指令:");

            // 指令下拉框
            int newCmdIndex = EditorGUI.Popup(new Rect(rect.x + labelWidth, rect.y, popupWidth, EditorGUIUtility.singleLineHeight), selectedCmdIndex, _cmdDropdownOptions);
            if (newCmdIndex != selectedCmdIndex && _concreteTypes.Count > 0)
            {
                Undo.RecordObject(_target, "Change Command");
                _target.handlers[index] = (CommandHandler)Activator.CreateInstance(_concreteTypes[newCmdIndex]);
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }

            // 描述
            EditorGUI.LabelField(new Rect(rect.x + labelWidth + popupWidth + 5, rect.y, rect.width - labelWidth - popupWidth - 5, EditorGUIUtility.singleLineHeight),
                                 $"描述: {_target.handlers[index].CmdDesc}", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.gray } });
        }

        private void OnAddItem(ReorderableList list)
        {
            Undo.RecordObject(_target, "Add Command");
            var newItem = _concreteTypes.Count > 0 ? (CommandHandler)Activator.CreateInstance(_concreteTypes[0]) : null;
            _target.handlers.Add(newItem);
            EditorUtility.SetDirty(_target);
            serializedObject.Update();
        }

        private void OnRemoveItem(ReorderableList list)
        {
            Undo.RecordObject(_target, "Remove Command");
            _target.handlers.RemoveAt(list.index);
            EditorUtility.SetDirty(_target);
            serializedObject.Update();
        }
    }
}
#endif