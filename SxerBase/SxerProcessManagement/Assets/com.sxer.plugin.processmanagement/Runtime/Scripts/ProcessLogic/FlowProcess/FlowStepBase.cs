using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement.Flow
{
    /// <summary>
    /// 顺序流程中的一个步骤（挂载在顺序流程物体子级下）
    /// </summary>
    public abstract class FlowStepBase : MonoBehaviour
    {
        [Tooltip("步骤名称，方便识别")]
        public string stepName;

        /// <summary>
        /// 步骤是否已完成（完成后顺序流程进入下一步骤）
        /// </summary>
        public bool IsComplete { get; protected set; }

        /// <summary>
        /// 标记当前步骤完成，将触发顺序流程切换到下一步骤
        /// </summary>
        public void CompleteStep()
        {
            IsComplete = true;
        }

        /// <summary>
        /// 重置步骤状态（供顺序流程复用）
        /// </summary>
        public virtual void ResetStep()
        {
            IsComplete = false;
        }

        // 由顺序流程调用的生命周期
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
    }
}