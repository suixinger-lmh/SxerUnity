using System;
using UnityEngine;

namespace Sxer.Plugin.ResourceSystem
{
    public enum ResourceState
    {
        None,
        Waiting,    // 等待加载（用于AddLoad延迟加载）
        Loading,    // 正在加载
        Success,    // 加载成功
        Failed      // 加载失败
    }

    public class ResourceHandle
    {
        public string Uri { get; private set; }
        public int RefCount { get; private set; }
        public bool Cacheable { get; private set; }
        public ResourceState State { get; internal set; }
        public ResourceProviderBase Provider { get; private set; }
       
        public Type ResourceType { get; private set; } // 新增：记录该句柄需要加载的具体资源类型
        public UnityEngine.Object Asset { get; internal set; }

        // 完成回调事件（用于合并加载完成事件，达到去重效果）
        private event Action<ResourceHandle> OnCompleteEvent;

        internal void Initialize(string uri, Type type, ResourceProviderBase provider, bool cacheable)
        {
            Uri = uri;
            ResourceType = type;
            Provider = provider;
            Cacheable = cacheable;
            State = ResourceState.None;
            RefCount = 0;
            Asset = null;
            OnCompleteEvent = null;
        }

        public void Retain()
        {
            RefCount++;
        }

        public void Release(bool forceClear = false)
        {
            if (forceClear)
            {
                RefCount = 0;
            }
            else
            {
                RefCount--;
            }

            if (RefCount <= 0)
            {
                ResourceManager.Instance.ReleaseHandle(this);
            }
        }

        // 注册回调（去重逻辑：如果已经在加载，直接注册回调即可）
        public void AddCallback(Action<ResourceHandle> callback)
        {
            if (callback == null) return;

            if (State == ResourceState.Success || State == ResourceState.Failed)
            {
                callback.Invoke(this); // 如果已经加载完，直接回调
            }
            else
            {
                OnCompleteEvent += callback;
            }
        }

        // 由Provider在加载完成时调用
        public void Complete(UnityEngine.Object asset, bool success)
        {
            Asset = asset;
            State = success ? ResourceState.Success : ResourceState.Failed;

            if (!success)
                Debug.LogError($"[ResourceManager] Failed to load resource: {Uri} of type {ResourceType}");

            OnCompleteEvent?.Invoke(this);
            OnCompleteEvent = null; // 释放引用

            // 如果在加载过程中被释放（RefCount <= 0），则在加载完成后执行延迟销毁
            if (RefCount <= 0)
            {
                ResourceManager.Instance.DelayedReleaseHandle(this);
            }
        }

        // 获取特定类型的资源
        public T Get<T>() where T : UnityEngine.Object
        {
            return Asset as T;
        }

        internal void Reset()
        {
            Uri = null;
            ResourceType = null;
            Provider = null;
            Asset = null;
            State = ResourceState.None;
            RefCount = 0;
            OnCompleteEvent = null;
        }
    }
}