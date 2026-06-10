using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Sxer.Editor.Project
{
    public partial class SxerProjectEditor
    {
        /// <summary>
        /// 创建多级文件夹
        /// </summary>
        static void CreateFolder(string folderPath, bool needLog = true)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                if (needLog)
                    Debug.Log("Folder Already Exist!(" + folderPath + ")");
                Ping(folderPath);
            }
            else
            {
                string subname = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
                string folderParent = folderPath.Remove(folderPath.LastIndexOf("/"));
                CreateFolder(folderParent, false);
                AssetDatabase.CreateFolder(folderParent, subname);
                Ping(folderPath);
            }
        }

        /// <summary>
        /// 高亮并选中资源（路径）
        /// </summary>
        static void Ping(string path)
        {
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
                Debug.LogError(path + "找不到！");
        }

        /// <summary>
        /// 高亮并选中对象（物体/资源）
        /// </summary>
        static void Ping(Object obj)
        {
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
                Debug.LogError("空对象！");
        }



    }
}