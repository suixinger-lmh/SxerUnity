using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


namespace Sxer.Plugin.ResourceSystem
{
    


    public class ResourceManager : MonoBehaviour
    {
        //避免只用资源地址作为key时，如果相同地址，加载不同类的资源，例如：一个图片地址，作为Texture2d加载和作为sprite加载。区分开
        private struct CacheKey : IEquatable<CacheKey>
        {
            public readonly string Uri;
            public readonly Type Type;

            public CacheKey(string uri, Type type)
            {
                Uri = uri;
                Type = type;
            }

            public bool Equals(CacheKey other) => Uri == other.Uri && Type == other.Type;
            public override bool Equals(object obj) => obj is CacheKey other && Equals(other);
            public override int GetHashCode() => (Uri != null ? Uri.GetHashCode() : 0) ^ (Type != null ? Type.GetHashCode() : 0);
        }



        public static ResourceManager Instance { get; private set; }

        private List<ResourceProviderBase> _providers = new List<ResourceProviderBase>();
        private Dictionary<CacheKey, ResourceHandle> _resourceCache = new Dictionary<CacheKey, ResourceHandle>();

        private IObjectPool<ResourceHandle> _handlePool;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
                InitializeProviders();
            }
            else
            {
                Destroy(gameObject);
            }
        }


        // 初始化内置对象池
        private void InitializePool()
        {
            _handlePool = new ObjectPool<ResourceHandle>(
                createFunc: () => new ResourceHandle(),              // 1. 当池空时，如何创建新对象
                actionOnGet: null,                                   // 2. 取出时执行 (我们有带参数的 Initialize，所以取出来后再手动初始化)
                actionOnRelease: handle => handle.Reset(),           // 3. 放回池中时执行 (清理数据，防止内存泄漏)
                actionOnDestroy: null,                               // 4. 超出最大容量时，销毁对象执行的逻辑 (纯C#类让GC回收即可)
                collectionCheck: true,                               // 5. 检查是否重复释放 (在Editor下防止同一个Handle被Release多次报错)
                defaultCapacity: 50,                                 // 6. 初始容量
                maxSize: 1000                                        // 7. 最大容量，超出后放入池中会被直接抛弃(GC)
            );
        }

        // 获取子物体下的所有Providers
        private void InitializeProviders()
        {
            _providers.AddRange(GetComponentsInChildren<ResourceProviderBase>());
            foreach (var provider in _providers)
            {
                provider.Init();
            }
        }

        // 路由分发
        private ResourceProviderBase GetProvider(string uri)
        {
            foreach (var provider in _providers)
            {
                if (uri.StartsWith(provider.RoutePrefix))
                    return provider;
            }
            Debug.LogError($"[ResourceManager] No provider found for URI: {uri}");
            return null;
        }



        // 核心获取逻辑（处理缓存和去重）
        private ResourceHandle GetHandleInternal(string uri, Type type, bool cacheable)
        {
            var key = new CacheKey(uri, type);
            // 1. 检查缓存
            if (_resourceCache.TryGetValue(key, out ResourceHandle cachedHandle))
            {
                cachedHandle.Retain();
                return cachedHandle;
            }

            // 2. 创建新Handle
            var provider = GetProvider(uri);
            if (provider == null) return null;

            var handle = _handlePool.Get();
            handle.Initialize(uri, type, provider, cacheable);
            handle.Retain();//记录第一次引用

            // 3. 写入缓存字典 (保证去重)
            if (cacheable)
            {
                _resourceCache[key] = handle;
            }

            return handle;
        }

        #region 公开 API

        // 只生成Handle，不立即加载 (延迟加载)
        public ResourceHandle AddLoad<T>(string uri, bool cacheable = true) where T : UnityEngine.Object
        {
            var handle = GetHandleInternal(uri, typeof(T), cacheable);
            if (handle != null && handle.State == ResourceState.None)
            {
                handle.State = ResourceState.Waiting;
            }
            return handle;
        }

        // 触发加载 (配合 AddLoad 使用)
        public void StartLoad(ResourceHandle handle, bool async = true)
        {
            if (handle == null || handle.State != ResourceState.Waiting) return;

            handle.State = ResourceState.Loading;
            if (async)
                handle.Provider.LoadAsync(handle);
            else
                handle.Provider.LoadSync(handle);
        }

        // 同步加载
        public ResourceHandle Load<T>(string uri, bool cacheable = true) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(uri))
            {
                Debug.LogError($"[ResourceManager] 资源地址为空！ {uri}");
                return null;
            }
                

            var handle = GetHandleInternal(uri, typeof(T), cacheable);

            if(handle == null)
            {
                Debug.LogError($"[ResourceManager] 资源Handle生成失败！");
                return null;
            }

            if (handle.State == ResourceState.None || handle.State == ResourceState.Waiting)
            {
                handle.State = ResourceState.Loading;
                handle.Provider.LoadSync(handle);
            }
            return handle;
        }

        // 异步加载
        public ResourceHandle LoadAsync<T>(string uri, Action<T> onComplete = null, bool cacheable = true) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(uri))
            {
                Debug.LogError($"[ResourceManager] 资源地址为空！ {uri}");
                return null;
            }

            var handle = GetHandleInternal(uri, typeof(T), cacheable);

            if (handle == null)
            {
                Debug.LogError($"[ResourceManager] 资源Handle生成失败！");
                return null;
            }

            // 绑定泛型回调
            if (onComplete != null)
            {
                handle.AddCallback(h => onComplete?.Invoke(h.Get<T>()));
            }

            // 如果是新创建的，或者是Waiting状态，则开始加载
            if (handle.State == ResourceState.None || handle.State == ResourceState.Waiting)
            {
                handle.State = ResourceState.Loading;
                handle.Provider.LoadAsync(handle);
            }

            return handle;
        }

        #endregion

        #region 释放 API

        // 内部释放逻辑，由 Handle.Release() 调用
        internal void ReleaseHandle(ResourceHandle handle)
        {
            if (handle == null) return;

            if (handle.Cacheable)
            {
                var key = new CacheKey(handle.Uri, handle.ResourceType);
                _resourceCache.Remove(key);
            }

            // 如果仍在加载中，不立即放回对象池，等待其 Load 完成后在 Complete() 中延迟释放
            if (handle.State == ResourceState.Loading || handle.State == ResourceState.Waiting)
            {
                return;
            }

            if (handle.State == ResourceState.Success)
            {
                handle.Provider.Unload(handle);
            }

            //handle.Reset();对象池事件已经添加了handle.reset()
            _handlePool.Release(handle);
        }


        // 延迟释放，仅由 ResourceHandle.Complete 内部在 RefCount <= 0 时调用
        internal void DelayedReleaseHandle(ResourceHandle handle)
        {
            if (handle.State == ResourceState.Success)
            {
                handle.Provider.Unload(handle);
            }
            _handlePool.Release(handle);
        }



        // 按地址释放
        public void Release<T>(string uri, bool forceClear = false) where T : UnityEngine.Object
        {
            var key = new CacheKey(uri, typeof(T));
            if (_resourceCache.TryGetValue(key, out ResourceHandle handle))
            {
                handle.Release(forceClear);
            }
        }

        // 按资源释放
        public void Release(UnityEngine.Object asset, bool forceClear = false)
        {
            if (asset == null) return;
            CacheKey targetKey = default;
            bool found = false;
            foreach (var kvp in _resourceCache)
            {
                if (kvp.Value.Asset == asset)
                {
                    targetKey = kvp.Key;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                _resourceCache[targetKey].Release(forceClear);
            }
            //if (targetUri != null)
            //    Release(targetUri, forceClear);
        }

        // 释放所有资源
        public void ReleaseAll()
        {
            List<ResourceHandle> handles = new List<ResourceHandle>(_resourceCache.Values);
            foreach (var handle in handles)
            {
                // 强制将引用归零并释放
                while (handle.RefCount > 0)
                {
                    handle.Release();
                }
            }
            _resourceCache.Clear();
        }
        #endregion
    }
}