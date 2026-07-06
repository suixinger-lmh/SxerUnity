using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sxer.Plugin.UISystem.Interfaces
{
    public interface IUIResourceLoader
    {
        // 异步加载并实例化UI预制体
        UniTask<GameObject> InstantiateAsync(string path, Transform parent = null);
        // 销毁并释放资源
        void Release(GameObject instance);
    }
}