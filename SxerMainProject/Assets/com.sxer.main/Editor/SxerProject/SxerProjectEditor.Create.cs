using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sxer.Editor.Project
{
    public partial class SxerProjectEditor
    {
        #region Create

        [MenuItem("Sxer/Project/Create/生成(选中)启动场景", priority = 0)]
        public static void CreateFrameStartScene()
        {
            string scenePath = string.Format("{0}/Scene/{1}", SxerFrameRoot, SceneName);
            Object obj = AssetDatabase.LoadMainAssetAtPath(scenePath);
            if (obj == null)
            {
                //生成文件夹
                CreateFolder(scenePath.Remove(scenePath.LastIndexOf('/')), false);
                //创建并保存场景
                Scene startScene = EditorSceneManager.NewScene(0);
                EditorSceneManager.SaveScene(startScene, scenePath);
                //打开场景
                startScene = EditorSceneManager.OpenScene(scenePath);
                //场景生成预制体(带关联)
                PrefabUtility.InstantiatePrefab(Resources.Load(Res_framePrefab), startScene);
                EditorSceneManager.MarkSceneDirty(startScene);//标记改动
                Ping(scenePath);
            }
            else
            {
                Debug.Log("启动场景已存在！");
                Ping(obj);
            }
        }

        [MenuItem("Sxer/Project/Create/生成项目文件夹", priority = 1)]
        public static void CreateProjectFiles()
        {
            CreateFolder("Assets/StreamingAssets");
            CreateFolder("Assets/MainProjectAssetsFile/Animations");
            CreateFolder("Assets/MainProjectAssetsFile/Scripts");
            CreateFolder("Assets/MainProjectAssetsFile/Models");
            CreateFolder("Assets/MainProjectAssetsFile/Prefabs");
            CreateFolder("Assets/MainProjectAssetsFile/UI");
            CreateFolder("Assets/MainProjectAssetsFile/Scenes");
            CreateFolder("Assets/MainProjectAssetsFile/Materials");
            CreateFolder("Assets/MainProjectAssetsFile/Textures");
            CreateFolder("Assets/MainProjectAssetsFile/CreateAssets");
            CreateFolder("Assets/MainProjectAssetsFile/Font");
            CreateFolder("Assets/MainProjectAssetsFile/Animators");
            CreateFolder("Assets/MainProjectAssetsFile/Temp");
            CreateFolder("Assets/MainProjectAssetsFile/Resources");
            Ping("Assets/MainProjectAssetsFile");
        }

        [MenuItem("Sxer/Project/Create/为当前场景生成布局", priority = 2)]
        public static void CreateSceneFrame()
        {
            Scene nowScene = SceneManager.GetActiveScene();
            string tag = "--------";
            new GameObject(string.Format("{0}{1}{2}", tag, "Camera", tag));
            new GameObject(string.Format("{0}{1}{2}", tag, "UI", tag));
            new GameObject(string.Format("{0}{1}{2}", tag, "Environment", tag));
            new GameObject(string.Format("{0}{1}{2}", tag, "Particle", tag));
            new GameObject(string.Format("{0}{1}{2}", tag, "Process", tag));
            EditorSceneManager.MarkSceneDirty(nowScene);
        }

        #endregion



        #region 生成代码

        // 菜单路径
        [MenuItem("Sxer/Project/Create/创建默认动态类", false, 100)]
        public static void CreateDefaultDynamicComponent()
        {
            // 1. 定义生成路径
            string scriptPath = "Assets/SxerFrame/Scripts/DefaultComponents";
            string fileName = "DefaultDynamicComponent.cs";
            string fullPath = Path.Combine(scriptPath, fileName);

            CreateFolder(scriptPath);

            // 3. 如果文件已存在，不覆盖（避免用户修改后被吞）
            if (File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("提示", "默认Logger已存在，无需重复创建！\n路径：" + fullPath, "确定");
                Ping(fullPath);
                return;
            }

            // 4. 生成代码内容
            string code = GenerateLoggerCode();

            // 5. 写入文件
            File.WriteAllText(fullPath, code);

            // 6. 刷新编辑器
            AssetDatabase.Refresh();

            // 7. 选中生成的文件，方便查看
            Ping(fullPath);

            //EditorUtility.DisplayDialog("创建成功",
            //    $"默认Logger创建完成！\n路径：{fullPath}\n\n已自动实现 SxerComponentLogger 抽象类",
            //    "确定");
        }

        /// <summary>
        /// 生成默认Logger实现代码
        /// </summary>
        private static string GenerateLoggerCode()
        {
            return @"

using Sxer.Frame;

// 自动生成的默认组件
[SxerComponentAttribute(""默认动态组件"", ComponentLifeType.DynamicInstance, ComponentInitType.Async)]
public class DefaultDynamicComponent : SxerDynamicComponent
{
    public override void OnDispose()
    {
        
    }

    public override bool OnInit()
    {
        return true;
    }
}
";
        }



        #endregion
    }
}