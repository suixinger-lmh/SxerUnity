using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using Sxer.Plugin.UISystem.Interfaces;

namespace Sxer.Plugin.UISystem
{
    public class ResourcesLoader : IUIResourceLoader
    {

        public async UniTask<GameObject> InstantiateAsync(string path, Transform parent = null)
        {
            // 使用原生异步加载，并用 UniTask 等待
            ResourceRequest request = Resources.LoadAsync<GameObject>(path);
            await request;

            if (request.asset == null)
            {
                Debug.LogError($"[UI Framework] 无法在 Resources/{path} 找到 UI 预制体!");
                return null;
            }

            GameObject instance = Object.Instantiate((GameObject)request.asset, parent);
            // 去掉 (Clone) 后缀，保持名字整洁
            instance.name = path;
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance != null)
            {
                Object.Destroy(instance);
            }
        }
    }
}