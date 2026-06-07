using UnityEngine;

namespace Sxer.Plugin.ResourceSystem
{
    public class UnityResourceProvider : ResourceProviderBase
    {
        public override string RoutePrefix => "res://";

        private string GetPath(string uri)
        {
            return uri.Replace(RoutePrefix, "");
        }

        public override void LoadSync(ResourceHandle handle)
        {
            var path = GetPath(handle.Uri);
            var asset = Resources.Load(path, handle.ResourceType);
            handle.Complete(asset, asset != null);
        }

        public override void LoadAsync(ResourceHandle handle)
        {
            var path = GetPath(handle.Uri);
            var request = Resources.LoadAsync(path, handle.ResourceType);

            request.completed += (asyncOp) =>
            {
                handle.Complete(request.asset, request.asset != null);
            };
        }

        public override void Unload(ResourceHandle handle)
        {
            if (handle.Asset != null && !(handle.Asset is GameObject))
            {
                // Resources 只能 Unload 非 GameObject(Component) 资源
                Resources.UnloadAsset(handle.Asset);
            }
        }
    }
}
