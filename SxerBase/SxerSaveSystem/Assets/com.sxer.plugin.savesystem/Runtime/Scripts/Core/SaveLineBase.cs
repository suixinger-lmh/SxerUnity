using System;

namespace Sxer.Plugin.SaveSystem
{
    public abstract class SaveLineBase
    {

        public string LineName { get; protected set; }
        public bool IsDirty { get; protected set; } // 脏标记：是否有未保存的修改


        protected ISaveSystem SaveSystem;

        // 该管线对应的专属文件夹目录
        protected string LineDirectory { get; private set; }

        public string FilePath { get; private set; }

        public string Key { get; private set; }

        // 当前正在操作的文件名
        protected string CurrentFileName { get; private set; }


        /// <summary>
        /// 由 Manager 在创建时调用，初始化管线基础依赖
        /// </summary>
        public virtual void Initialize(string name,ISaveSystem saveSystem, string filePath, string defaultKey)
        {
            LineName = name;
            SaveSystem = saveSystem;
            FilePath = filePath;
            Key = defaultKey;
        }


        ///// <summary>
        ///// 获取当前完整的文件路径
        ///// </summary>
        //protected string GetFullPath() => Path.Combine(LineDirectory, CurrentFileName);


        
        /// <summary>
        /// 切换文件或者键
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        public virtual void Switch(string filePath, string key) 
        {
            FilePath = filePath;
            Key = key;
        }
        /// <summary>
        /// 删除当前聚焦的文件
        /// </summary>
        public virtual void Delete()
        {
            SaveSystem.Delete(FilePath, Key);
        }


        // 强制子类实现具体的保存和读取逻辑，因为只有子类知道要存什么数据实体
        
        public abstract void Save();
        public abstract void Load();
        public abstract void Release();

        //public Action OnSetFile;
        //public Action OnSaveDeleted;
        //public Action OnCollectSaveData;
        //public Action OnRestoreFailureDetected;

        //public SaveLine(string name,string path,SaveSystemBase saveSystem)
        //{
        //    this.name = name;
        //    this.currentFilePath = path;
        //    this.saveSystem = saveSystem;
        //}
    }



    // 泛型基类：负责管理具体的数据类型 TData
    public abstract class SaveLine<T> : SaveLineBase where T : class, new() 
    {
        // 核心：内存缓存数据！所有的游戏系统都读取和修改这里的数据
        public T Data { get; protected set; }

        public event Action<T> OnLoaded;
        public event Action<T> OnSaved;


        public virtual void ChangeCacheData(T data) 
        {
            Data = data;
            MarkDirty();
        }

        public override void Initialize(string name, ISaveSystem saveSystem, string filePath, string defaultKey)
        {
            base.Initialize(name, saveSystem, filePath, defaultKey);
            Data = new T();
        }
        public void MarkDirty() => IsDirty = true; // 外部修改数据后调用

        public override void Save()
        {
            SaveSystem.Save(FilePath, Key, Data);
            IsDirty = false;
            OnSaved?.Invoke(Data);
        }

        public override void Load()
        {
            Data = SaveSystem.Load<T>(FilePath, Key);
            IsDirty = false;
            OnLoaded?.Invoke(Data);
        }
        public override void Release()
        {
            Data = null;
            OnLoaded = null;
            OnSaved = null;
        }

    }



}