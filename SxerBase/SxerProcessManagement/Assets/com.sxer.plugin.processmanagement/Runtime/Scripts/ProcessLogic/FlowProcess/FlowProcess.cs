using System.Collections.Generic;
using UnityEngine;

namespace Sxer.Plugin.ProcessManagement.Flow
{
    /// <summary>
    /// 顺序流程变体：挂载此脚本的物体，会自动收集所有子物体上的FlowStepBase，
    /// 并按照Hierarchy中的顺序依次执行。
    /// </summary>
    public class FlowProcess : ProcessBase
    {
        [Tooltip("是否循环执行（完成后回到第一步）")]
        public bool loop = false;

        [SerializeField]
        private List<FlowStepBase> steps = new List<FlowStepBase>();
        private int currentStepIndex = -1;

        private FlowStepBase currentStep;


        /// <summary>
        /// 在编辑器中重新收集步骤（可通过自定义编辑器调用）
        /// </summary>
        public void RefreshSteps()
        {
            steps.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var step = transform.GetChild(i).GetComponent<FlowStepBase>();
                if (step != null) steps.Add(step);
            }
        }

        
        /// <summary>
        /// 初始化获取一次所有步骤，并对步骤进行初始化
        /// </summary>
        protected override void OnInit()
        {
            RefreshSteps();
            foreach (var step in steps)
            {
                step.InitStep();
            }
        }

        protected override void OnStart()
        {
            if (steps.Count == 0)
            {
                Debug.LogWarning($"[FlowProcess] {processId} 没有子级步骤，直接完成");
                isCompleted = true;
                return;
            }
            currentStepIndex = 0;
            currentStep = steps[currentStepIndex];
            currentStep.ResetStep();
            currentStep.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (currentStep == null) return;

            currentStep.OnUpdate(deltaTime); // 传递时间

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
                        isCompleted = true;
                    }
                }
            }
        }

        // 补全父节点被打断/暂停时的向下穿透逻辑
        protected override void OnPause() => currentStep?.OnPause();
        protected override void OnResume() => currentStep?.OnResume();


        protected override void OnStop()
        {
            currentStep?.OnLeave();
            currentStepIndex = -1;
            currentStep = null;
        }

        public List<FlowStepBase> GetSteps() => steps;
    }
}