using UnityEngine;

namespace Sxer.Plugin.ResourceSystem
{
    /// <summary>
    /// 资源加载器，
    /// 提供同步和异步资源加载
    /// 管理ResourceHandle
    /// </summary>
    public abstract class ResourceProviderBase: MonoBehaviour
    {
        // 路由前缀，例如 "res://", "addr://", "web://"
        public abstract string RoutePrefix { get; }

        // 初始化（可在此处做一些加载器配置）
        public virtual void Init() { }

        // 同步加载
        public abstract void LoadSync(ResourceHandle handle);

        // 异步加载
        public abstract void LoadAsync(ResourceHandle handle);

        // 卸载资源
        public abstract void Unload(ResourceHandle handle);
    }
}