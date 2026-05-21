using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sxer.Frame
{
    public partial class SxerFrame : MonoBehaviour
    {
        public static string PrefabPath = "SxerFramePrefab/SxerFrame";
        public static FrameState CurrentState { get; private set; } = FrameState.None;

        [SerializeField] private bool autoInit = true;

        private static bool _isAppQuitting = false;

        public static Action<string> OnFrameLog = Debug.Log;

        // 分类存储：单例字典 + 动态列表
        private readonly Dictionary<Type, SxerComponentBase> _singletonComponents = new Dictionary<Type, SxerComponentBase>();
        private readonly List<SxerComponentBase> _dynamicComponents = new List<SxerComponentBase>();

        private static SxerFrame _instance;
        public static SxerFrame Instance
        {
            get
            {
                if (_isAppQuitting) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SxerFrame>();
                    if (_instance == null)
                    {
                        var prefab = Resources.Load<GameObject>(PrefabPath);
                        if (prefab)
                            _instance = Instantiate(prefab).GetComponent<SxerFrame>();
                        else
                            FrameLog("SxerFrame框架资源获取异常！生成失败！请检查资源地址！");
                    }
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (autoInit && CurrentState == FrameState.None) StartupFramework();
        }

        private void Update()
        {
            if (CurrentState != FrameState.Running) return;

            // 更新单例
            foreach (var comp in _singletonComponents.Values)
                if (comp.ComponentState == ComponentState.Inited && comp.InitResult)
                    comp.OnUpdate(Time.deltaTime);

            // 更新动态组件（倒序遍历防删除报错）
            for (int i = _dynamicComponents.Count - 1; i >= 0; i--)
            {
                var comp = _dynamicComponents[i];
                if (comp.ComponentState == ComponentState.Inited && comp.InitResult)
                    comp.OnUpdate(Time.deltaTime);
            }
        }

        public void StartupFramework()
        {
            if (CurrentState != FrameState.None) return;
            CurrentState = FrameState.Initializing;
            StartCoroutine(InitProcess());
        }

        private IEnumerator InitProcess()
        {
            FrameLog("【搜集单例组件】...");
            var foundComponents = GetComponentsInChildren<SxerComponentBase>(true);

            foreach (var comp in foundComponents)
            {
                var attr = comp.GetType().GetCustomAttribute<SxerComponentAttribute>();
                if (attr != null)
                {
                    comp.Priority = attr.InitPriority;
                    comp.InitType = attr.InitType;
                    comp.LifeType = attr.LifeType;
                }

                if (comp.LifeType == ComponentLifeType.GlobalSingleton)
                    _singletonComponents[comp.GetType()] = comp;
            }

            FrameLog("【单例组件初始化】...");
            var sorted = _singletonComponents.Values.OrderBy(c => c.Priority).ToList();
            foreach (var comp in sorted)
            {
                yield return StartCoroutine(comp.FrameworkInit(null));
            }

            CurrentState = FrameState.Running;
            FrameLog("【框架启动完毕】 Running!");
        }

        private void OnDestroy() 
        { 
            if (this == _instance) 
                if (!_isAppQuitting) 
                    DestroyFramework(); 
        }

        private void OnApplicationQuit()
        {
            _isAppQuitting = true;
            DestroyFramework();
        }

        public void DestroyFramework()
        {
            if (CurrentState == FrameState.Destroyed || CurrentState == FrameState.DisposeIng) return;
            CurrentState = FrameState.DisposeIng;
            StartCoroutine(DisposeProcess());
        }

        private IEnumerator DisposeProcess()
        {
            // 优先释放动态组件
            for (int i = _dynamicComponents.Count - 1; i >= 0; i--)
                yield return StartCoroutine(_dynamicComponents[i].FrameworkDispose());
            _dynamicComponents.Clear();

            // 逆序释放单例组件
            var sorted = _singletonComponents.Values.OrderByDescending(c => c.Priority).ToList();
            foreach (var comp in sorted)
                yield return StartCoroutine(comp.FrameworkDispose());
            _singletonComponents.Clear();

            CurrentState = FrameState.Destroyed;
            _instance = null;
            Destroy(gameObject);
        }

        internal static void FrameLog(string msg)
        {
            OnFrameLog?.Invoke($"[SxerFrame] {msg}");
        }
    }
}