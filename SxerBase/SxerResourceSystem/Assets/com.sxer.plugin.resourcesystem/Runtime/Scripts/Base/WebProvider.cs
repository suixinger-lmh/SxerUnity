using Sxer.Plugin.ResourceSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Sxer.Plugin.ResourceSystem
{
    public class WebProvider : ResourceProviderBase
    {
        public override string RoutePrefix => "http";

        public override void LoadSync(ResourceHandle handle) {
            Debug.LogWarning($"[WebProvider] 强烈建议不要同步加载网络资源，这会卡死主线程！URI: {handle.Uri}");
            handle.Complete(null, false);
        }

        public override void LoadAsync(ResourceHandle handle)
        {
            StartCoroutine(DownloadRoutine(handle));
        }

        private IEnumerator DownloadRoutine(ResourceHandle handle)
        {
            UnityWebRequest request = null;



            // 根据泛型指定的类型，动态创建对应的底层请求器
            if (handle.ResourceType == typeof(Texture2D))
            {
                request = UnityWebRequestTexture.GetTexture(handle.Uri);
            }
            else if (handle.ResourceType == typeof(AudioClip))
            {
                request = UnityWebRequestMultimedia.GetAudioClip(handle.Uri, AudioType.UNKNOWN);
            }
            else if(handle.ResourceType == typeof(AssetBundle))
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(handle.Uri);
            }
            else
            {
                Debug.LogError($"[WebProvider] 不支持的请求类型: {handle.ResourceType}");
                handle.Complete(null, false);
                yield break;
            }

            using (request)
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Object asset = null;
                    // 根据类型获取结果
                    if (handle.ResourceType == typeof(Texture2D))
                    {
                        asset = DownloadHandlerTexture.GetContent(request);
                    }
                    else if (handle.ResourceType == typeof(AudioClip))
                    {
                        asset = DownloadHandlerAudioClip.GetContent(request);
                    }
                    handle.Complete(asset, true);
                }
                else
                {
                    Debug.LogError($"[WebProvider] 下载失败: {request.error}");
                    handle.Complete(null, false);
                }
            }
        }

        public override void Unload(ResourceHandle handle)
        {
            if (handle.Asset != null) Destroy(handle.Asset);
        }
    }
}