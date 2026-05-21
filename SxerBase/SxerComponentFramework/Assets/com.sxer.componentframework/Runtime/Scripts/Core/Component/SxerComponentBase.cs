using System;
using System.Collections;
using UnityEngine;

namespace Sxer.Frame
{
    public abstract class SxerComponentBase : MonoBehaviour
    {
        public string ComponentID => GetType().FullName;
        public virtual int Priority { get; set; } = 100;
        public ComponentInitType InitType { get; set; } = ComponentInitType.Sync;
        public ComponentLifeType LifeType { get; set; } = ComponentLifeType.DynamicInstance;
        public ComponentState ComponentState { get; protected set; } = ComponentState.UnInit;
        public virtual bool InitResult { get; set; } = false;

        #region 生命周期抽象与虚方法
        public abstract bool OnInit();
        public abstract void OnDispose();
        public virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnBeforeReload() { }

        public virtual IEnumerator OnInitAsync()
        {
            InitResult = OnInit();
            yield break;
        }

        public virtual IEnumerator OnDisposeAsync()
        {
            OnDispose();
            yield break;
        }
        #endregion

        #region 框架内部调度
        internal IEnumerator FrameworkInit(Action<bool, SxerComponentBase> onComplete)
        {
            if (ComponentState != ComponentState.UnInit) yield break;

            ComponentState = ComponentState.InitIng;
            yield return StartCoroutine(OnInitAsync());

            ComponentState = ComponentState.Inited;
            onComplete?.Invoke(InitResult, this);
        }

        /// <summary>
        /// 框架内部调用：热重载组件
        /// </summary>
        internal IEnumerator FrameworkReload(Action<bool, SxerComponentBase> onComplete = null)
        {
            // 1. 如果组件正在运行，先执行预清理和卸载
            if (ComponentState == ComponentState.Inited || ComponentState == ComponentState.InitIng)
            {
                ComponentState = ComponentState.ReloadIng;
                OnBeforeReload(); // 触发业务层的数据保存/预处理

                yield return StartCoroutine(OnDisposeAsync());
                ComponentState = ComponentState.UnInit;
            }

            // 2. 重新执行初始化流程
            ComponentState = ComponentState.InitIng;
            yield return StartCoroutine(OnInitAsync());

            ComponentState = ComponentState.Inited;

            // 3. 记录日志并回调
            SxerFrame.FrameLog($"组件重载完成 [{ComponentID}]，结果: {InitResult}");
            onComplete?.Invoke(InitResult, this);
        }

        internal IEnumerator FrameworkDispose(Action<SxerComponentBase> onComplete = null)
        {
            if (ComponentState == ComponentState.Inited)
            {
                ComponentState = ComponentState.DisposeIng;
                yield return StartCoroutine(OnDisposeAsync());
                ComponentState = ComponentState.UnInit;
            }
            onComplete?.Invoke(this);
        }

        internal IEnumerator FrameworkDestroy()
        {
            if (ComponentState == ComponentState.Destroyed) yield break;
            yield return StartCoroutine(FrameworkDispose());
            ComponentState = ComponentState.Destroyed;
            if (gameObject != null) Destroy(gameObject);
        }
        #endregion
    }
}