using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public enum ProcessState
    {
        Inactive,   // 未启动
        Running,    // 运行中
        Paused,     // 已暂停
        Completed   // 已完成
    }

    /// <summary>
    /// 流程基类，所有流程变体均继承于此。
    /// 挂载到场景物体上，通过ProcessManager统一管理。
    /// </summary>
    public abstract class ProcessBase : MonoBehaviour
    {
        [Header("流程标识")]
        [Tooltip("唯一标识，请保持全场不重复")]
        public string processId;

        [Tooltip("流程描述，便于区分同类变体实例")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("是否在场景收集后自动启动（需调用ProcessManager.CollectAndAutoStart）")]
        public bool autoExecute = false;    // 新增：自动执行标记

        public ProcessState State { get; protected set; } = ProcessState.Inactive;

        private bool isInitialized = false;

        public void Init()
        {
            if (isInitialized) return;
            isInitialized = true;
            OnInit();
        }

        /// <summary>
        /// 流程开始（由Manager调用）
        /// </summary>
        public void StartProcess()
        {
            if (State == ProcessState.Running) return;
            State = ProcessState.Running;
            OnStart();
        }

        /// <summary>
        /// 流程停止（由Manager调用）
        /// </summary>
        public void StopProcess()
        {
            if (State == ProcessState.Inactive) return;
            OnStop();
            State = ProcessState.Inactive;
        }

        /// <summary>
        /// 暂停流程（由Manager调用）
        /// </summary>
        public void PauseProcess()
        {
            if (State != ProcessState.Running) return;
            State = ProcessState.Paused;
            OnPause();
        }

        /// <summary>
        /// 恢复流程（由Manager调用）
        /// </summary>
        public void ResumeProcess()
        {
            if (State != ProcessState.Paused) return;
            State = ProcessState.Running;
            OnResume();
        }

        /// <summary>
        /// 由ProcessManager在Update中驱动（仅Running状态）
        /// </summary>
        public void UpdateProcess(float deltaTime)
        {
            if (State == ProcessState.Running)
                OnUpdate(deltaTime);
        }


        public void CompleteProcess()
        {
            
        }


        // 子类重写的生命周期方法
        protected virtual void OnInit() { }
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnUpdate(float deltaTime) { }

        protected virtual void OnCompleted() { }

    }
}