using UnityEngine;

namespace Sxer.Plugin.ProcessManagement.Flow
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
        public bool IsComplete => isCompleted;
        protected bool isCompleted = false;

        private bool isInitialized = false;

        internal void InitStep()
        {
            if (isInitialized) return;
            isInitialized = true;
            OnInit();
        }


   

        /// <summary>
        /// 重置步骤状态（供顺序流程复用）
        /// </summary>
        internal void ResetStep()
        {
            isCompleted = false;
            OnReset();
        }


        /// <summary> 只在最开始初始化一次 </summary>
        protected virtual void OnInit() { }
        /// <summary> 每次重新进入该步骤前调用 </summary>
        protected virtual void OnReset() { }
        /// <summary> 进入该步骤 </summary>
        public virtual void OnEnter() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnLeave() { }
        public virtual void OnPause() { }
        public virtual void OnResume() { }
    }
}