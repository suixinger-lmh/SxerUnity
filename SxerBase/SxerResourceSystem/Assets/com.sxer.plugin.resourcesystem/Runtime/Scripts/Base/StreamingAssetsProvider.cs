using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Sxer.Plugin.ResourceSystem
{
    /// <summary>
    /// StreamingAssets文件夹加载器（PC平台专用）
    /// PC平台StreamingAssets路径：Application.streamingAssetsPath (file:///xxx/StreamingAssets)
    /// 支持类型：TextAsset/Texture2D/AudioClip/Byte[]
    /// </summary>
    public class StreamingAssetsProvider : ResourceProviderBase
    {
        // 资源路径前缀：stream://xxx (xxx为StreamingAssets内相对路径)
        public override string RoutePrefix => "stream://";

        /// <summary>
        /// 拼接PC平台StreamingAssets完整路径
        /// </summary>
        private string GetStreamingAssetsPath(string uri)
        {
            var relativePath = uri.Replace(RoutePrefix, "");
            // PC平台StreamingAssets路径拼接（自动处理file://前缀）
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        /// <summary>
        /// 同步加载（PC平台StreamingAssets同步加载需用UnityWebRequest阻塞方式）
        /// </summary>
        public override void LoadSync(ResourceHandle handle)
        {
            Debug.LogWarning($"[StreamingAssetsProvider] 强烈建议不要同步加载网络资源，这会卡死主线程！URI: {handle.Uri}");
            handle.Complete(null, false);

            //var fullPath = GetStreamingAssetsPath(handle.Uri);
            //// PC平台必须用UnityWebRequest访问StreamingAssets（即使是本地文件）
            //using (var request = CreateWebRequest(fullPath, handle.ResourceType))
            //{
            //    // 同步阻塞请求
            //    var operation = request.SendWebRequest();
            //    while (!operation.isDone) { } // 直接卡死，request依赖update，这里直接卡死

            //    ProcessWebRequestResult(request, handle);
            //}
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        public override void LoadAsync(ResourceHandle handle)
        {
            

            var fullPath = GetStreamingAssetsPath(handle.Uri);
            UnityWebRequest request = CreateWebRequest(fullPath, handle.ResourceType);
            var op = request.SendWebRequest();
            op.completed += _ =>
            {
                ProcessWebRequestResult(request, handle);
                request.Dispose();
            };

        }

        public override void Unload(ResourceHandle handle)
        {
            if (handle.Asset != null)
            {
                Destroy(handle.Asset); // 释放动态创建的 Texture2D / AudioClip
            }
        }

        /// <summary>
        /// 根据资源类型创建对应的UnityWebRequest
        /// </summary>
        private UnityWebRequest CreateWebRequest(string path, Type resourceType)
        {
            // 处理PC平台file路径前缀
            var uri = new System.Uri(path);
            var requestUrl = uri.AbsoluteUri;

            if (resourceType == typeof(Texture2D))
            {
                return UnityWebRequestTexture.GetTexture(requestUrl);
            }
            else if (resourceType == typeof(AudioClip))
            {
                return UnityWebRequestMultimedia.GetAudioClip(requestUrl, AudioType.UNKNOWN);
            }
            else if (resourceType == typeof(TextAsset) || resourceType == typeof(byte[]))
            {
                return UnityWebRequest.Get(requestUrl);
            }
            else
            {
                throw new System.NotSupportedException($"[StreamingAssetsProvider] Unsupported type: {resourceType.Name}");
            }
        }

        /// <summary>
        /// 处理WebRequest结果并完成Handle
        /// </summary>
        private void ProcessWebRequestResult(UnityWebRequest request, ResourceHandle handle)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[StreamingAssetsProvider] Request failed: {request.error} | Url: {request.url}");
                handle.Complete(null, false);
                return;
            }

            UnityEngine.Object asset = null;
            if (handle.ResourceType == typeof(Texture2D))
            {
                asset = DownloadHandlerTexture.GetContent(request);
            }
            else if (handle.ResourceType == typeof(AudioClip))
            {
                asset = DownloadHandlerAudioClip.GetContent(request);
            }
            else if (handle.ResourceType == typeof(TextAsset))
            {
                var text = request.downloadHandler.text;
                asset = new TextAsset(text);
            }
            //else if (handle.ResourceType == typeof(byte[]))
            //{
            //    var bytes = request.downloadHandler.data;
            //    asset = new TextAsset(bytes); // 用TextAsset包装二进制数据
            //}

            //if (asset != null)
            //{
            //    asset.name = System.IO.Path.GetFileName(handle.Uri);
            //}

            handle.Complete(asset, asset != null);
        }

   

    }
}