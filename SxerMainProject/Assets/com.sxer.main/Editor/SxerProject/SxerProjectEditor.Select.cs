using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace Sxer.Editor.Project
{
    public partial class SxerProjectEditor
    {
        #region Select Folder

        [MenuItem("Sxer/Project/Select/Folder/StreamingAssets", priority = 5)]
        public static void SelectFolder_StreamingAssets()
        {
            Ping("Assets/StreamingAssets");
        }

        [MenuItem("Sxer/Project/Select/Folder/Animations", priority = 5)]
        public static void SelectFolder_Animations()
        {
            Ping("Assets/MainProjectAssetsFile/Animations");
        }

        [MenuItem("Sxer/Project/Select/Folder/Animators", priority = 5)]
        public static void SelectFolder_Animators()
        {
            Ping("Assets/MainProjectAssetsFile/Animators");
        }

        [MenuItem("Sxer/Project/Select/Folder/UI", priority = 5)]
        public static void SelectFolder_UI()
        {
            Ping("Assets/MainProjectAssetsFile/UI");
        }

        [MenuItem("Sxer/Project/Select/Folder/Prefabs", priority = 5)]
        public static void SelectFolder_Prefabs()
        {
            Ping("Assets/MainProjectAssetsFile/Prefabs");
        }

        [MenuItem("Sxer/Project/Select/Folder/Scenes", priority = 5)]
        public static void SelectFolder_Scenes()
        {
            Ping("Assets/MainProjectAssetsFile/Scenes");
        }

        [MenuItem("Sxer/Project/Select/Folder/Scripts", priority = 5)]
        public static void SelectFolder_Scripts()
        {
            Ping("Assets/MainProjectAssetsFile/Scripts");
        }

        #endregion


        #region Select FrameObject

        [MenuItem("Sxer/Project/Select/Frame Entrance Logic Type", priority = 0)]
        public static void SelectFrame_LogicType()
        {
            //判断当前场景
            string scenePath = string.Format("{0}/Scene/{1}", SxerFrameRoot, SceneName);
            if (EditorSceneManager.GetSceneByPath(scenePath) == EditorSceneManager.GetActiveScene())
            {
                GameObject logicObj = GameObject.Find("Sxer_Frame/CoreComponent/ProcessLogic");
                Ping(logicObj);
            }
            else
            {
                if (EditorUtility.DisplayDialog("切换当前场景", "确定切换到启动场景吗？请确保当前场景已保存", "确认"))
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(scenePath);
                    if (obj != null)
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        GameObject logicObj = GameObject.Find("Sxer_Frame/CoreComponent/ProcessLogic");
                        Ping(logicObj);
                    }
                    else
                    {
                        Debug.LogError("启动场景不存在！");
                    }
                }
            }
        }

        #endregion
    }
}
