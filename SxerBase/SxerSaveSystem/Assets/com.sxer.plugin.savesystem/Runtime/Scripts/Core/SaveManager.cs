
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sxer.Plugin.SaveSystem
{
    /// <summary>
    /// 统一管理 数据存储
    /// 存档
    /// 配置文件
    /// 
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }


        private Dictionary<string, SaveLineBase> saveLines = new Dictionary<string, SaveLineBase>();

        [SerializeField]
        private List<ISaveSystem> saveSystemList = new List<ISaveSystem>();
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Init();
        }

        public void Init()
        {
            saveSystemList = GetComponentsInChildren<ISaveSystem>().ToList();
        }

        /// <summary>
        /// 生成对应存储管线
        /// </summary>
        /// <typeparam name="T">具体的存储管线</typeparam>
        /// <typeparam name="S">绑定的低层存储系统</typeparam>
        /// <param name="name">管线名称</param>
        /// <param name="filePath">文件地址</param>
        /// <param name="key">文件存储key</param>
        /// <returns></returns>
        public T CreateSaveLine<T,S>(string name, string filePath,string key) where T : SaveLineBase, new() where S: ISaveSystem
        {
            if (saveLines.ContainsKey(name))
            {
                return saveLines[name] as T;
            }


            ISaveSystem saveSystem = saveSystemList.FirstOrDefault(p => p.GetType() == typeof(S));
            if (saveSystem == null)
            {
                Debug.LogError($"不存在的存储系统{typeof(S)}");
                return null;
            }

            T tempLine = new T();

            tempLine.Initialize(name, saveSystem, filePath, key);

            saveLines.Add(name, tempLine);
            return tempLine;
        }


        public void AddSaveSystem(ISaveSystem saveSystem) {
            saveSystemList.Add(saveSystem);
        }


        //private string GetFileFolderPath()
        //{

        //}


        public T GetSaveLine<T>(string name) where T : SaveLineBase
        {
            saveLines.TryGetValue(name, out var line);
            return line as T;
        }

        public bool RemoveSaveLine(string name)
        {
            if (saveLines.TryGetValue(name, out var line))
            {
                line.Release();
                return saveLines.Remove(name);
            }
            return false;
        }

        public void SaveAllDirtyLines()
        {
            foreach (var line in saveLines.Values)
            {
                if (line.IsDirty) line.Save();
            }
        }

        public IEnumerable<string> GetAllSaveLineNames() => saveLines.Keys;

    }
}