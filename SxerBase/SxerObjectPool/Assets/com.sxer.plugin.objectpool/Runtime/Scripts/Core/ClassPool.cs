using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Pool; // 依赖 Unity 官方基础池，它是纯 C# 实现的，极高效率

namespace Sxer.Plugin.ObjectPool
{
    /// <summary>
    /// 纯 C# 类对象池生命周期接口
    /// 建议重置数据的逻辑写在 OnDespawn 中，防止脏数据残留
    /// </summary>
    public interface IClassPoolable
    {
        void OnSpawn();   // 取出时调用
        void OnDespawn(); // 回收时调用
    }

    /// <summary>
    /// 纯 C# 类泛型对象池
    /// 强制要求 T 必须是引用类型 (class) 且拥有无参构造函数 (new())
    /// </summary>
    public class ClassPool<T> where T : class, new()
    {
        private readonly ObjectPool<T> _pool;
        private readonly List<T> _activeObjects;

        // 生命周期委托扩展
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly Action<T> _onCreate;

        /// <summary>
        /// 获取当前所有正在使用的对象只读集合
        /// </summary>
        public ReadOnlyCollection<T> ActiveEntries => _activeObjects.AsReadOnly();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ClassPool(
            Action<T> onGet = null, Action<T> onRelease = null,
            Action<T> onDestroy = null, Action<T> onCreate = null,
            bool collectionCheck = true, int defaultCapacity = 50, int maxSize = 5000)
        {
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _onCreate = onCreate;

            _activeObjects = new List<T>(defaultCapacity);

            // 核心：使用原生对象池管理纯 C# 内存分配
            _pool = new ObjectPool<T>(
                CreateInstance,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck,
                defaultCapacity,
                maxSize);
        }

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T item)
        {
            if (item == null) return;
            _pool.Release(item);
        }

        // --- 内部底层绑定逻辑 ---

        private T CreateInstance()
        {
            T instance = new T(); // 纯 C# 实例化，完全没有 Unity 引擎开销
            _onCreate?.Invoke(instance);
            return instance;
        }

        private void OnGet(T item)
        {
            _activeObjects.Add(item);

            // 触发接口
            if (item is IClassPoolable poolable)
            {
                poolable.OnSpawn();
            }

            _onGet?.Invoke(item);
        }

        private void OnRelease(T item)
        {
            _activeObjects.Remove(item);

            // 触发接口
            if (item is IClassPoolable poolable)
            {
                poolable.OnDespawn();
            }

            _onRelease?.Invoke(item);
        }

        private void OnDestroy(T item)
        {
            // 对于纯 C# 对象，没有 Destroy() 方法，直接让其失去引用等待系统 GC 即可
            _onDestroy?.Invoke(item);
        }

        // --- 批量管理功能 ---

        /// <summary>
        /// 回收所有未回收的对象（清空当前活跃列表并入池）
        /// 极度适合切场景、清空未处理完的事件队列等操作
        /// </summary>
        public void ReleaseAll()
        {
            _activeObjects.RemoveAll(e => e == null);

            foreach (T item in _activeObjects.ToArray())
            {
                Release(item);
            }
        }

        /// <summary>
        /// 彻底清空对象池及所有引用（释放内存）
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
            _activeObjects.Clear();
        }
    }
}