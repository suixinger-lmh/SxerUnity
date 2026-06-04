using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool; // 依赖 Unity 2021+ 引入的官方高性能池

//这个是《鸭科夫》里基于UnityEngine.Pool封装的对象池管理，主要是针对组件进行封装使用
//可以做为参考，以后用到时自己封装自已要的对象池

namespace Sxer.Plugin.ObjectPool
{
    /// <summary>
    /// 池化对象生命周期接口
    /// </summary>
    public interface IPoolable
    {
        void NotifyPooled();   // 相当于 OnSpawn，出池时调用
        void NotifyReleased(); // 相当于 OnDespawn，入池时调用
    }

    /// <summary>
    /// 泛型预制体对象池（强制要求泛型 T 必须是 Unity 组件）
    /// </summary>
    public class PrefabPool<T> where T : Component
    {
        public readonly T Prefab;
        public Transform poolParent;

        public readonly bool CollectionCheck;
        public readonly int DefaultCapacity;
        public readonly int MaxSize;

        private readonly ObjectPool<T> pool;
        private readonly List<T> activeObjects;

        // 回调委托，用于外部扩展生命周期
        private Action<T> onGet;
        private Action<T> onRelease;
        private Action<T> onDestroy;
        private Action<T> onCreate;

        /// <summary>
        /// 获取当前所有正在活跃（已出池）的对象只读集合
        /// </summary>
        public ReadOnlyCollection<T> ActiveEntries => activeObjects.AsReadOnly();

        public PrefabPool(T prefab, Transform poolParent = null,
            Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, Action<T> onCreate = null,
            bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            Prefab = prefab;
            prefab.gameObject.SetActive(false);

            // 如果未指定父节点，则默认使用预制体原本的父节点
            this.poolParent = poolParent != null ? poolParent : prefab.transform.parent;

            this.onGet = onGet;
            this.onRelease = onRelease;
            this.onDestroy = onDestroy;
            this.onCreate = onCreate;

            CollectionCheck = collectionCheck;
            DefaultCapacity = defaultCapacity;
            MaxSize = maxSize;

            // 核心：封装 Unity 原生的 ObjectPool
            pool = new ObjectPool<T>(CreateInstance, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize);
            activeObjects = new List<T>();
        }

        public T Get(Transform setParent = null)
        {
            Transform targetParent = setParent != null ? setParent : poolParent;

            T item = pool.Get();

            if (targetParent)
            {
                item.transform.SetParent(targetParent, false);
                item.transform.SetAsLastSibling();
            }

            return item;
        }

        public void Release(T item)
        {
            pool.Release(item);

            // 触发入池接口
            if (item is IPoolable poolable)
            {
                poolable.NotifyReleased();
            }
        }

        // --- 以下为对 Unity 原生池逻辑的内部绑定 ---

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(Prefab);
            onCreate?.Invoke(instance);
            return instance;
        }

        private void OnGet(T item)
        {
            activeObjects.Add(item);
            item.gameObject.SetActive(true);

            // 触发出池接口
            if (item is IPoolable poolable)
            {
                poolable.NotifyPooled();
            }

            onGet?.Invoke(item);
        }

        private void OnRelease(T item)
        {
            activeObjects.Remove(item);
            onRelease?.Invoke(item);

            if (item != null)
            {
                item.gameObject.SetActive(false);
                item.transform.SetParent(poolParent);
            }
        }

        private void OnDestroy(T item)
        {
            onDestroy?.Invoke(item);
            if (item != null)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }

        // --- 批量管理功能 ---

        /// <summary>
        /// 强制回收所有活跃对象（常用语切场景、重开游戏）
        /// </summary>
        public void ReleaseAll()
        {
            activeObjects.RemoveAll(e => e == null); // 清理意外销毁的空引用

            // 必须转换为 Array 遍历，防止在 Release 中修改集合引发异常
            foreach (T item in activeObjects.ToArray())
            {
                Release(item);
            }
        }

        /// <summary>
        /// 查找符合条件的活跃对象
        /// </summary>
        public T Find(Predicate<T> predicate)
        {
            return activeObjects.Find(predicate);
        }

        /// <summary>
        /// 回收所有符合条件的活跃对象
        /// </summary>
        public int ReleaseAll(Predicate<T> predicate)
        {
            List<T> listToRelease = activeObjects.FindAll(predicate);
            foreach (T item in listToRelease)
            {
                Release(item);
            }
            return listToRelease.Count;
        }
    }
}