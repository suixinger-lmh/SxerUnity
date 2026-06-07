using System;
using UnityEngine;

namespace Sxer.Plugin.ResourceSystem
{
    public static class ResourceManagerExtensions
    {
        
        public static string RoutePrefix_Web(this string str)
        {
            return "http" + str;
        }

        public static string RoutePrefix_Resource(this string str)
        {
            return "res://" + str;
        }

        public static string RoutePrefix_Addr(this string str)
        {
            return "addr://" + str;
        }

        public static string RoutePrefix_StreamingAssets(this string str)
        {
            return "stream://" + str;
        }

        public static string RoutePrefix_LocalPath(this string str)
        {
            return "file://" + str;
        }


        ///// <summary>
        ///// 同步实例化
        ///// 实例化完成后，直接释放资源句柄，不进行长期缓存
        ///// </summary>
        //public static GameObject Instantiate(this ResourceManager manager, string uri, Transform parent = null)
        //{
        //    // 走标准加载，Manager内部会Retain(+1)
        //    var handle = manager.Load<GameObject>(uri);
        //    GameObject instance = null;

        //    if (handle.State == ResourceState.Success && handle.Asset != null)
        //    {
        //        var prefab = handle.Get<GameObject>();
        //        instance = GameObject.Instantiate(prefab, parent);
        //    }
        //    else
        //    {
        //        Debug.LogError($"[ResourceManager] Instantiate Failed, URI: {uri}");
        //    }

        //    // 实例化完毕，直接释放资源句柄(-1)
        //    // 如果没有其他地方持有该句柄，资源将立刻被 Provider 卸载
        //    handle.Release();

        //    return instance;
        //}
        ///// <summary>
        ///// 异步实例化
        ///// 加载并实例化完成后，直接释放资源句柄，不进行长期缓存
        ///// </summary>
        //public static void InstantiateAsync(this ResourceManager manager, string uri, Action<GameObject> onComplete, Transform parent = null)
        //{
        //    // AddLoad 内部会进行缓存检查和去重，并 Retain(+1)
        //    var handle = manager.AddLoad<GameObject>(uri);

        //    handle.AddCallback(h =>
        //    {
        //        GameObject instance = null;
        //        if (h.State == ResourceState.Success && h.Asset != null)
        //        {
        //            var prefab = h.Get<GameObject>();
        //            instance = GameObject.Instantiate(prefab, parent);
        //        }
        //        else
        //        {
        //            Debug.LogError($"[ResourceManager] InstantiateAsync Failed, URI: {uri}");
        //        }

        //        // 实例化后立刻释放资源 (-1)
        //        h.Release();

        //        // 执行用户侧的回调
        //        onComplete?.Invoke(instance);
        //    });

        //    // 触发加载 (如果已经在加载中，StartLoad 内部会忽略，仅等待回调)
        //    manager.StartLoad(handle, true);
        //}




    }


}