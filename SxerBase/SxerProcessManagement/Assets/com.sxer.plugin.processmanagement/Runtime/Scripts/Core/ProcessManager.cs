using System.Collections.Generic;
using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement
{
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
        // 当前活动的流程
        private ProcessBase currentProcess;

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

            // 检查 autoExecute 被勾选的数量
            List<ProcessBase> autoCandidates = new List<ProcessBase>();
            foreach (var proc in foundProcesses)
            {
                if (proc.autoExecute)
                    autoCandidates.Add(proc);
            }

            if (autoCandidates.Count > 1)
            {
                Debug.LogError("ProcessManager: 场景中存在多个流程勾选了 autoExecute，只能有一个流程自动启动！" +
                               "请检查以下物体的 ProcessBase 组件：" +
                               string.Join(", ", autoCandidates.ConvertAll(p => p.gameObject.name)));
            }
            else if (autoCandidates.Count == 1)
            {
                SwitchProcess(autoCandidates[0].processId);
            }
            // 如果 0 个，什么都不做
        }



        private void Update()
        {
            // 驱动当前流程的更新
            currentProcess?.UpdateProcess(Time.deltaTime);
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

            if (processes.ContainsKey(process.processId))
            {
                Debug.LogWarning($"流程ID重复，已覆盖: {process.processId}");
                processes[process.processId] = process;
            }
            else
            {
                processes.Add(process.processId, process);
            }

            processes[process.processId].Init();
        }

        /// <summary>
        /// 注销流程
        /// </summary>
        public void Unregister(string processId)
        {
            if (processes.ContainsKey(processId))
            {
                // 如果要注销的是当前活动流程，先停止
                if (currentProcess != null && currentProcess.processId == processId)
                {
                    currentProcess.StopProcess();
                    currentProcess = null;
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
                Debug.LogError($"流程未注册: {processId}");
                return false;
            }

            // 停止当前流程
            if (currentProcess != null && currentProcess != newProcess)
            {
                currentProcess.StopProcess();
            }

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
    }
}