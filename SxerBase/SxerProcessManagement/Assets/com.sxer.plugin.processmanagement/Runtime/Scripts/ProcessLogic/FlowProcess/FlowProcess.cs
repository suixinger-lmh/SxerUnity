using System.Collections.Generic;
using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement.Flow
{
    /// <summary>
    /// 顺序流程变体：挂载此脚本的物体，会自动收集所有子物体上的FlowStepBase，
    /// 并按照Hierarchy中的顺序依次执行。
    /// </summary>
    public class FlowProcess : ProcessBase
    {
        [Tooltip("是否循环执行（完成后回到第一步）")]
        public bool loop = false;

        private List<FlowStepBase> steps = new List<FlowStepBase>();
        private int currentStepIndex = -1;

        private FlowStepBase currentStep;

        private void CollectSteps()
        {
            currentStepIndex = -1;
            currentStep = null;
            steps.Clear();
            // 只收集直接子物体上的步骤，并按SiblingIndex排序
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var step = child.GetComponent<FlowStepBase>();
                if (step != null)
                {
                    steps.Add(step);
                }
            }
        }

        /// <summary>
        /// 在编辑器中重新收集步骤（可通过自定义编辑器调用）
        /// </summary>
        public void RefreshSteps()
        {
            CollectSteps();
        }

        protected override void OnInit()
        {
            base.OnInit();

            RefreshSteps();
        }

        protected override void OnStart()
        {
            if (steps.Count == 0)
            {
                Debug.LogWarning($"顺序流程 [{processId}] 没有子级步骤，直接完成");
                State = ProcessState.Completed;
                OnCompleted();
                return;
            }
            currentStepIndex = 0;
            currentStep = steps[currentStepIndex];
            currentStep.ResetStep();
            currentStep.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (steps.Count == 0 || currentStepIndex < 0 || currentStepIndex >= steps.Count)
                return;

            if (currentStep == null)
                return;
            
            currentStep.OnUpdate();

            if (currentStep.IsComplete)
            {
                // 离开当前步骤
                currentStep.OnLeave();

                // 判断是否还有下一步骤
                if (currentStepIndex + 1 < steps.Count)
                {
                    currentStepIndex++;
                    currentStep = steps[currentStepIndex];
                    currentStep.ResetStep();
                    currentStep.OnEnter();
                }
                else
                {
                    if (loop)
                    {
                        // 循环：回到第一步
                        currentStepIndex = 0;
                        currentStep = steps[currentStepIndex];
                        currentStep.ResetStep();
                        currentStep.OnEnter();
                    }
                    else
                    {
                        // 流程完成
                        State = ProcessState.Completed;
                        OnCompleted();
                    }
                }
            }
        }

        protected override void OnStop()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
            {
                currentStep = steps[currentStepIndex];
                currentStep.OnLeave();
            }
            currentStepIndex = -1;
            currentStep = null;
        }

      
    }
}