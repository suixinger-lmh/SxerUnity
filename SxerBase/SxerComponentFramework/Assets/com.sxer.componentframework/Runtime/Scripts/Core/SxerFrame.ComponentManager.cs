using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sxer.Frame
{
    public partial class SxerFrame
    {

        #region 单例组件 API
        /// <summary>
        /// 获取单例组件
        /// </summary>
        public T GetSingletonComponent<T>() where T : SxerComponentBase
        {
            if (_singletonComponents.TryGetValue(typeof(T), out var comp))
            {
                if (comp.ComponentState != ComponentState.Inited)
                {
                    FrameLog($"组件未初始化完成：{typeof(T).FullName}");
                    return null;
                }
                else if (!comp.InitResult)
                {
                    FrameLog($"组件初始化失败！：{typeof(T).FullName}");
                    return null;
                }

                return (T)comp;
            }
            else
            {
                FrameLog($"未找到组件：{typeof(T).FullName}");
                return null;
            }
        }
        #endregion

        #region 动态组件 API (新增的核心功能)
        /// <summary>
        /// 动态添加并初始化一个组件到指定 GameObject
        /// </summary>
        public T AddDynamicComponent<T>(GameObject target) where T : SxerDynamicComponent
        {
            T comp = target.GetComponent<T>();
            if(comp == null)
            {
                comp = target.AddComponent<T>();
            }
            return AddDynamicComponent(comp);
        }
        public T AddDynamicComponent<T>(T comp) where T : SxerDynamicComponent 
        {
            if(comp!=null)
            {
                if (!_dynamicComponents.Contains(comp))
                {
                    _dynamicComponents.Add(comp);
                    // 如果框架已经在运行，立刻触发该组件的初始化
                    if (CurrentState == FrameState.Running)
                    {
                        StartCoroutine(comp.FrameworkInit(null));
                    }
                    return comp;
                }

                FrameLog($"该组件已经添加！请勿重复添加：{typeof(T).FullName}");
                return null;
            }
            FrameLog($"组件为空！无法添加！{typeof(T).FullName}");
            return null;
        }
        /// <summary>
        /// 卸载并销毁动态组件
        /// </summary>
        public void RemoveDynamicComponent(SxerDynamicComponent comp)
        {
            if (_dynamicComponents.Contains(comp))
            {
                _dynamicComponents.Remove(comp);
                StartCoroutine(comp.FrameworkDestroy()); // 走异步释放后销毁
            }
        }

        /// <summary>
        /// 获取所有指定类型的动态组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inited">true只获取初始化完成的，false获取全部组件</param>
        /// <returns></returns>
        public List<T> GetDynamicComponents<T>(bool inited = true) where T : SxerDynamicComponent
        {
            if (inited)
                return _dynamicComponents.OfType<T>().Where(c => c.ComponentState == ComponentState.Inited).ToList();
            else
                return _dynamicComponents.OfType<T>().ToList();
        }
        #endregion


        #region 场景级生命周期管理 (清理动态组件)

        /// <summary>
        /// 清理所有动态组件（用于场景切换前调用）
        /// </summary>
        private IEnumerator ClearAllDynamicComponentsAsync()
        {
            FrameLog("【场景切换】开始清理所有动态组件...");

            // 倒序遍历，防止在释放过程中修改了列表导致报错
            for (int i = _dynamicComponents.Count - 1; i >= 0; i--)
            {
                var comp = _dynamicComponents[i];
                if (comp != null)
                {
                    // 异步释放每一个动态组件并销毁其 GameObject（如果需要保留 GameObject 可以调用 FrameworkDispose）
                    yield return StartCoroutine(comp.FrameworkDestroy());
                }
            }

            _dynamicComponents.Clear();
            FrameLog("【场景切换】动态组件清理完毕！全局单例保留。");
        }

        /// <summary>
        /// 同步方式调用清理（不等待内部协程完成，直接触发，适用于不严格要求时序的轻度清理）
        /// </summary>
        public void ClearAllDynamicComponents()
        {
            StartCoroutine(ClearAllDynamicComponentsAsync());
        }

        #endregion

        #region 组件异常恢复与热重载 (Reload)

        /// <summary>
        /// 热重载指定的单例组件
        /// </summary>
        public void ReloadSingletonComponent<T>() where T : SxerSingletonComponent<T>
        {
            var comp = GetSingletonComponent<T>();
            if (comp != null)
            {
                FrameLog($"请求热重载单例组件: {typeof(T).Name}");
                StartCoroutine(comp.FrameworkReload());
            }
            else
            {
                FrameLog($"重载失败：找不到单例组件 {typeof(T).Name}");
            }
        }

        /// <summary>
        /// 重载指定的动态组件
        /// </summary>
        public void ReloadDynamicComponent(SxerDynamicComponent comp)
        {
            if (_dynamicComponents.Contains(comp))
            {
                FrameLog($"请求重载动态组件: {comp.ComponentID}");
                StartCoroutine(comp.FrameworkReload());
            }
        }

        #endregion
    }
}