using Sxer.Plugin.ResourceSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Sxer.Plugin.ResourceSystem
{
    public class AddressablesProvider : ResourceProviderBase
    {
        public override string RoutePrefix => "addr://";

        private string GetAddress(string uri)
        {
            return uri.Replace(RoutePrefix, "");
        }

        public override void LoadSync(ResourceHandle handle)
        {
            var address = GetAddress(handle.Uri);
            var op = Addressables.LoadAssetAsync<Object>(address);
            var asset = op.WaitForCompletion(); // Í¬²½µÈ´ý
            handle.Complete(asset, asset != null);
        }

        public override void LoadAsync(ResourceHandle handle)
        {
            var address = GetAddress(handle.Uri);
            Addressables.LoadAssetAsync<Object>(address).Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    handle.Complete(op.Result, true);
                }
                else
                {
                    handle.Complete(null, false);
                }
            };
        }

        public override void Unload(ResourceHandle handle)
        {
            if (handle.Asset != null)
            {
                Addressables.Release(handle.Asset);
            }
        }
    }
}