using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sxer.Plugin.ProcessManagement
{
    public enum ManagerMode
    {
        Manual,     // 手动模式（流程结束后静默停止）
        AutoQueue   // 自动队列模式（流程结束后拉起下一个）
    }


    /// <summary>
    /// 全局流程管理器，整个程序生命周期仅一个实例。
    /// 负责收集、切换、暂停、恢复所有流程。
    /// 
    /// TODO:
    /// 监控
    /// </summary>
    public class ProcessManager : MonoBehaviour
    {

        [SerializeField, Tooltip("启动后默认执行的流程ID，可为空")]
        private string defaultProcessId;

        // 所有已注册的流程
        private Dictionary<string, ProcessBase> processes = new Dictionary<string, ProcessBase>();

        // 自动执行队列
        private Queue<ProcessBase> autoExecutionQueue = new Queue<ProcessBase>();

        // 当前活动的流程
        private ProcessBase currentProcess;

        // 当前管理器运行模式
        public ManagerMode CurrentMode { get; private set; } = ManagerMode.Manual;

        public void Run()
        {
            // 如果设置了默认流程，自动启动
            if (!string.IsNullOrEmpty(defaultProcessId) && processes.ContainsKey(defaultProcessId))
            {
                SwitchProcess(defaultProcessId);
            }
        }


        /// <summary>
        /// 收集场景中所有 ProcessBase（含未激活的）并注册，然后检查 autoExecute 标记，
        /// 仅当恰好有一个标记为 true 时自动启动该流程，否则报错或忽略。
        /// </summary>
        public void CollectAndAutoStart()
        {
            // 搜索场景中所有 ProcessBase 实例（包括未激活物体）
            ProcessBase[] foundProcesses = FindObjectsByType<ProcessBase>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            // 先注册所有找到的流程（避免重复，Register 会处理）
            foreach (var proc in foundProcesses)
            {
                Register(proc);
            }

            // 2. 提取需要自动执行的流程，按优先级排序（由小到大）
            var autoProcesses = foundProcesses
                .Where(p => p.autoExecute)
                .OrderBy(p => p.priority)
                .ToList();
            // 3. 入队
            autoExecutionQueue.Clear();
            foreach (var proc in autoProcesses)
            {
                autoExecutionQueue.Enqueue(proc);
            }
            // 4. 开始执行队列中的第一个
            CurrentMode = ManagerMode.AutoQueue;
            ExecuteNextInQueue();

            //// 检查 autoExecute 被勾选的数量
            //List<ProcessBase> autoCandidates = new List<ProcessBase>();
            //foreach (var proc in foundProcesses)
            //{
            //    if (proc.autoExecute)
            //        autoCandidates.Add(proc);
            //}

            //if (autoCandidates.Count > 1)
            //{
            //    Debug.LogError("ProcessManager: 场景中存在多个流程勾选了 autoExecute，只能有一个流程自动启动！" +
            //                   "请检查以下物体的 ProcessBase 组件：" +
            //                   string.Join(", ", autoCandidates.ConvertAll(p => p.gameObject.name)));
            //}
            //else if (autoCandidates.Count == 1)
            //{
            //    SwitchProcess(autoCandidates[0].processId);
            //}
            //// 如果 0 个，什么都不做
        }



        private void Update()
        {
            if (currentProcess == null) return;
            // 1. 驱动子类逻辑
            currentProcess.UpdateProcess(Time.deltaTime);

            // 2. 嗅探子类状态（如果子类内部逻辑判定完成，将 isCompleted 设为 true，这里就会捕捉到）
            if (currentProcess.IsCompleted)
            {
                ProcessCompletedRoutine(currentProcess);
            }
        }
        /// <summary>
        /// 内部流转收尾逻辑：接管原先的事件回调
        /// </summary>
        private void ProcessCompletedRoutine(ProcessBase completedProcess)
        {
            // 触发基类的收尾生命周期
            completedProcess.MarkAsCompleted();
            currentProcess = null;

            // 队列拉起
            if (CurrentMode == ManagerMode.AutoQueue)
            {
                ExecuteNextInQueue();
                return;
            }
        }

        private void OnDestroy()
        {
            // 防止销毁时产生内存/事件泄漏
            ReleaseAll();
        }



        /// <summary>
        /// 注册流程（由ProcessBase自动调用）
        /// </summary>
        public void Register(ProcessBase process)
        {
            if (process == null || string.IsNullOrEmpty(process.processId))
            {
                Debug.LogWarning("流程注册失败：ID为空或对象为空");
                return;
            }
            processes[process.processId] = process;
            process.Init();
        }

        /// <summary>
        /// 注销流程
        /// </summary>
        public void Unregister(string processId)
        {
            if (processes.TryGetValue(processId, out ProcessBase proc))
            {
                // 如果要注销的是当前活动流程，先停止
                if (currentProcess == proc)
                {
                    StopCurrent();
                }
                processes.Remove(processId);
            }
        }

        /// <summary>
        /// 切换到指定ID的流程（停止当前，启动目标）
        /// </summary>
        public bool SwitchProcess(string processId)
        {
            if (!processes.TryGetValue(processId, out var newProcess))
            {
                Debug.LogError($"[ProcessManager] 流程切换失败，未注册: {processId}");
                return false;
            }

            // 漏洞修复：如果目标流程正是当前正在运行的流程，忽略操作
            if (currentProcess == newProcess)
            {
                return true;
            }

            // 强行切换意味着打断自动化与挂起栈，回归手动单点控制
            CurrentMode = ManagerMode.Manual;

            // 停止当前流程
            StopCurrent();

            currentProcess = newProcess;
            currentProcess.StartProcess();
            return true;
        }

        /// <summary>
        /// 暂停当前活动流程
        /// </summary>
        public void PauseCurrent()
        {
            currentProcess?.PauseProcess();
        }

        /// <summary>
        /// 恢复当前活动流程
        /// </summary>
        public void ResumeCurrent()
        {
            currentProcess?.ResumeProcess();
        }

        /// <summary>
        /// 停止当前活动流程
        /// </summary>
        public void StopCurrent()
        {
            if (currentProcess != null)
            {
                currentProcess.StopProcess();
                currentProcess = null;
            }
        }

        /// <summary>
        /// 获取当前活动流程（可能为null）
        /// </summary>
        public ProcessBase GetCurrentProcess() => currentProcess;

        /// <summary>
        /// 根据ID获取已注册流程
        /// </summary>
        public ProcessBase GetProcess(string id)
        {
            processes.TryGetValue(id, out var p);
            return p;
        }

        /// <summary>
        /// 完全释放与清理：停止当前流程、清空队列、注销所有事件、清空字典
        /// </summary>
        public void ReleaseAll()
        {
            StopCurrent();
            autoExecutionQueue.Clear();

            processes.Clear();
            Debug.Log("[ProcessManager] 所有流程已清理释放。");
        }




        /// <summary>
        /// 执行队列中的下一个流程（内部逻辑使用）
        /// </summary>
        private void ExecuteNextInQueue()
        {
            if (autoExecutionQueue.Count > 0)
            {
                currentProcess = autoExecutionQueue.Dequeue();
                currentProcess.StartProcess();
            }
            else
            {
                CurrentMode = ManagerMode.Manual; // 队列消耗完毕，转回手动
                Debug.Log("[ProcessManager] 自动执行队列已全部完成。");
            }
        }

      


    }
}