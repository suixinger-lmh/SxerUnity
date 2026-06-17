using UnityEngine;
using UnityEditor;
using System.IO;

namespace Sxer.Plugin.ProcessManagement.Editor
{
    public class StepScriptGenerator : EditorWindow
    {
        private string scriptName = "MyNewFlowStep";
        private string savePath = "Assets/SxerFrame/Scripts/FlowSteps"; // 默认路径

        [MenuItem("Sxer/Process/FlowProcess/创建新 Step 脚本 (Generate Step)")]
        public static void ShowWindow()
        {
            var window = GetWindow<StepScriptGenerator>(true, "生成 Step 脚本", true);
            window.minSize = new Vector2(350, 150);
            window.maxSize = new Vector2(350, 150);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            scriptName = EditorGUILayout.TextField("脚本类名:", scriptName);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("保存路径:", savePath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string folder = EditorUtility.OpenFolderPanel("选择保存目录", savePath, "");
                if (!string.IsNullOrEmpty(folder))
                {
                    // 转换为相对路径
                    if (folder.StartsWith(Application.dataPath))
                        savePath = "Assets" + folder.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);
            if (GUILayout.Button("生成并编译", GUILayout.Height(30)))
            {
                GenerateScript();
            }
        }

        private void GenerateScript()
        {
            if (string.IsNullOrEmpty(scriptName) || !char.IsLetter(scriptName[0]))
            {
                EditorUtility.DisplayDialog("错误", "类名不合法！首字母必须为字母。", "确定");
                return;
            }

            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            string fullPath = $"{savePath}/{scriptName}.cs";
            if (File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("错误", $"已存在同名脚本: {scriptName}", "确定");
                return;
            }

            string template =
$@"using UnityEngine;
using Sxer.Plugin.ProcessManagement.Flow;

public class {scriptName} : FlowStepBase
{{
    protected override void OnInit()
    {{
        // 初始获取组件（仅一次）
    }}

    public override void OnEnter()
    {{
        // 进入此步骤时执行
        Debug.Log(""进入步骤: "" + stepName);
    }}

    public override void OnUpdate(float deltaTime)
    {{
        // 帧更新逻辑
        // 如果满足完成条件：
        // isCompleted = true;
    }}

    public override void OnLeave()
    {{
        // 离开此步骤时清理
    }}
}}";
            File.WriteAllText(fullPath, template);
            AssetDatabase.Refresh();

            Object obj = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
            Close();
        }
    }
}