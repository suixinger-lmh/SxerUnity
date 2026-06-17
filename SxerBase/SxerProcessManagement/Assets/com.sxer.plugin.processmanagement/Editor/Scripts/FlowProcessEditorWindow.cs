using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sxer.Plugin.ProcessManagement.Flow;

namespace Sxer.Plugin.ProcessManagement.Editor
{
    // 1. 劫持默认 Inspector，只留一个打开窗口的按钮
    [CustomEditor(typeof(FlowProcess))]
    public class FlowProcessInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(10);
            if (GUILayout.Button("🛠️ 打开 Flow 流程编辑器", GUILayout.Height(40)))
            {
                FlowProcessEditorWindow.OpenWindow((FlowProcess)target);
            }
        }
    }

    // 2. 独立的编辑窗口
    public class FlowProcessEditorWindow : EditorWindow
    {
        private FlowProcess targetProcess;
        private FlowStepBase selectedStep;
        private UnityEditor.Editor cachedStepEditor; // 用于在右侧绘制选中步骤的Inspector

        private Vector2 leftScroll;
        private Vector2 rightScroll;

        public static void OpenWindow(FlowProcess process)
        {
            var window = GetWindow<FlowProcessEditorWindow>("Flow 编辑器");
            window.targetProcess = process;
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            if (targetProcess == null)
            {
                EditorGUILayout.HelpBox("请先在场景中选中一个 FlowProcess。", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"正在编辑流程: {targetProcess.gameObject.name} [{targetProcess.processId}]", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            // --- 左侧：步骤列表管理 ---
            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            DrawStepList();
            EditorGUILayout.EndVertical();

            // --- 右侧：选中步骤的具体内容编辑 ---
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            DrawStepDetails();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStepList()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("步骤列表", EditorStyles.boldLabel);
            if (GUILayout.Button("+ 添加步骤", GUILayout.Width(80)))
            {
                ShowAddStepMenu();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            var steps = targetProcess.GetSteps();
            if (steps == null || steps.Count == 0)
            {
                EditorGUILayout.HelpBox("当前无步骤，请点击右上角添加。", MessageType.Info);
                return;
            }

            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null) continue;

                bool isSelected = (selectedStep == step);
                GUI.backgroundColor = isSelected ? new Color(0.6f, 0.8f, 1f) : Color.white;

                EditorGUILayout.BeginHorizontal("box");

                // 选择按钮
                if (GUILayout.Button($"{i + 1}. {step.stepName ?? step.GetType().Name}", EditorStyles.label, GUILayout.ExpandWidth(true)))
                {
                    selectedStep = step;
                    GUI.FocusControl(null); // 清除焦点，刷新右侧
                }

                // 上下移动与删除
                GUI.backgroundColor = Color.white;
                GUI.enabled = i > 0;
                if (GUILayout.Button("↑", GUILayout.Width(25))) MoveStep(step, i - 1);
                GUI.enabled = i < steps.Count - 1;
                if (GUILayout.Button("↓", GUILayout.Width(25))) MoveStep(step, i + 1);
                GUI.enabled = true;

                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("删除确认", $"确定删除 {step.gameObject.name} 吗？", "删除", "取消"))
                    {
                        if (selectedStep == step) selectedStep = null;
                        Undo.DestroyObjectImmediate(step.gameObject);
                        targetProcess.RefreshSteps();
                        GUIUtility.ExitGUI();
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawStepDetails()
        {
            if (selectedStep == null)
            {
                EditorGUILayout.HelpBox("请在左侧选择一个步骤以编辑其详细内容。", MessageType.Info);
                return;
            }

            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
            EditorGUILayout.LabelField($"编辑步骤: {selectedStep.GetType().Name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 动态创建选中的 Step 的检视面板并绘制
            UnityEditor.Editor.CreateCachedEditor(selectedStep, null, ref cachedStepEditor);
            if (cachedStepEditor != null)
            {
                cachedStepEditor.OnInspectorGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        private void MoveStep(FlowStepBase step, int newIndex)
        {
            step.transform.SetSiblingIndex(newIndex);
            targetProcess.RefreshSteps();
            Repaint(); // 刷新窗口
        }

        private void ShowAddStepMenu()
        {
            GenericMenu menu = new GenericMenu();
            var stepTypes = TypeCache.GetTypesDerivedFrom<FlowStepBase>()
                .Where(t => !t.IsAbstract && !t.IsGenericType);

            foreach (var type in stepTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddStepToProcess(type));
            }
            menu.ShowAsContext();
        }

        private void AddStepToProcess(Type type)
        {
            // 自动在 FlowProcess 下生成子物体
            GameObject stepObj = new GameObject(type.Name);
            Undo.RegisterCreatedObjectUndo(stepObj, "Add Flow Step");
            stepObj.transform.SetParent(targetProcess.transform);
            stepObj.transform.localPosition = Vector3.zero;

            var stepComponent = stepObj.AddComponent(type) as FlowStepBase;
            stepComponent.stepName = type.Name;

            targetProcess.RefreshSteps();
            selectedStep = stepComponent; // 自动选中新生成的步骤
            Repaint();
        }
    }
}