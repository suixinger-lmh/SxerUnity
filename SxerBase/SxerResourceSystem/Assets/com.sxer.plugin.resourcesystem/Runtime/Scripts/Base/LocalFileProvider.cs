using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Sxer.Plugin.ResourceSystem
{
    public class LocalFileProvider : ResourceProviderBase
    {
        public override string RoutePrefix => "file://";

        private string GetLocalPath(string uri)
        {
            return uri.Replace(RoutePrefix, "");
        }

        public override void LoadSync(ResourceHandle handle)
        {
            string path = GetLocalPath(handle.Uri);
            if (!File.Exists(path))
            {
                Debug.LogError($"[LocalFileProvider] File not found: {path}");
                handle.Complete(null, false);
                return;
            }

            try
            {
                if (handle.ResourceType == typeof(Texture2D))
                {
                    byte[] data = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(data))
                    {
                        handle.Complete(tex, true);
                        return;
                    }
                }
                else if (handle.ResourceType == typeof(TextAsset))
                {
                    string text = File.ReadAllText(path);
                    handle.Complete(new TextAsset(text), true);
                    return;
                }
                else
                {
                    Debug.LogWarning($"[LocalFileProvider] {handle.ResourceType} 暂不支持同步原生读取，请使用异步加载。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalFileProvider] Sync load failed: {ex.Message}");
            }

            handle.Complete(null, false);
        }

        public override void LoadAsync(ResourceHandle handle)
        {
            string path = GetLocalPath(handle.Uri);
            string url = "file://" + path; // 拼接符合 UnityWebRequest 的 file:// 协议地址

            UnityWebRequest request = CreateWebRequest(url, handle.ResourceType);
            var op = request.SendWebRequest();
            op.completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    UnityEngine.Object asset = ExtractAsset(request, handle.ResourceType);
                    handle.Complete(asset, asset != null);
                }
                else
                {
                    Debug.LogError($"LocalFileProvider 异步加载失败: {handle.Uri} - {request.error}");
                    handle.Complete(null, false);
                }
                request.Dispose();
            };
        }

        public override void Unload(ResourceHandle handle)
        {
            if (handle.Asset != null)
                Destroy(handle.Asset);
        }

        private UnityWebRequest CreateWebRequest(string url, Type type)
        {
            if (type == typeof(Texture2D))
                return UnityWebRequestTexture.GetTexture(url);
            if (type == typeof(AudioClip))
                return UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            return UnityWebRequest.Get(url);
        }

        private UnityEngine.Object ExtractAsset(UnityWebRequest request, Type type)
        {
            if (type == typeof(Texture2D))
                return DownloadHandlerTexture.GetContent(request);
            if (type == typeof(AudioClip))
                return DownloadHandlerAudioClip.GetContent(request);
            if (type == typeof(TextAsset))
                return new TextAsset(request.downloadHandler.text);
            return null;
        }
    }
}