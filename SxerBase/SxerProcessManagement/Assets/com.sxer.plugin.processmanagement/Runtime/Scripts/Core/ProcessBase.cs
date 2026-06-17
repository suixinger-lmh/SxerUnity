using System;
using UnityEngine;

namespace Sxer.Plugin.ProcessManagement
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

        [Tooltip("自动执行优先级（数值越小，越先执行）")]
        public int priority = 0;


        public ProcessState State { get; protected set; } = ProcessState.Inactive;

        // 流程完成事件（给业务逻辑扩展下）
        public event Action<ProcessBase> OnProcessCompletedEvent;


        // 核心修改：暴露给Manager的只读属性，子类通过 protected 变量修改
        public bool IsCompleted => isCompleted;
        protected bool isCompleted = false;


        private bool isInitialized = false;

        internal void Init()
        {
            if (isInitialized) return;
            isInitialized = true;
            OnInit();
        }

        /// <summary>
        /// 流程开始（由Manager调用）
        /// </summary>
        internal void StartProcess()
        {
            if (State == ProcessState.Running || State == ProcessState.Paused)
            {
                Debug.LogWarning($"[ProcessBase] 流程 {processId} 当前状态为 {State}，无法直接Start。");
                return;
            }
            isCompleted = false; // 每次启动时重置完成状态
            State = ProcessState.Running;
            OnStart();
        }

        /// <summary>
        /// 流程停止（由Manager调用）
        /// </summary>
        internal void StopProcess()
        {
            if (State == ProcessState.Inactive) return;
            OnStop();
            State = ProcessState.Inactive;
        }

        /// <summary>
        /// 暂停流程（由Manager调用）
        /// </summary>
        internal void PauseProcess()
        {
            if (State != ProcessState.Running) return;
            State = ProcessState.Paused;
            OnPause();
        }

        /// <summary>
        /// 恢复流程（由Manager调用）
        /// </summary>
        internal void ResumeProcess()
        {
            if (State != ProcessState.Paused) return;
            State = ProcessState.Running;
            OnResume();
        }

        /// <summary>
        /// 由ProcessManager在Update中驱动（仅Running状态）
        /// </summary>
        internal void UpdateProcess(float deltaTime)
        {
            if (State == ProcessState.Running)
                OnUpdate(deltaTime);
        }


        // 由 Manager 确认完成后，统一调用的收尾动作
        internal void MarkAsCompleted()
        {
            if (State == ProcessState.Completed) return;
            State = ProcessState.Completed;
            OnCompleted();
            OnProcessCompletedEvent?.Invoke(this);
        }

        //public void CompleteProcess()
        //{
        //    if (State == ProcessState.Completed) return;

        //    // 如果流程还在运行或暂停，先触发停止相关逻辑(视业务需求可选)，这里直接切完成态
        //    State = ProcessState.Completed;
        //    OnCompleted();

        //    // 通知Manager该流程已完成，可以执行下一个
        //    OnProcessCompletedEvent?.Invoke(this);
        //}


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