using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Xml.Serialization;


namespace Sxer.Plugin.ProcessManagement.XMLProcess
{

    [System.Serializable]
    [XmlType("OperationBase")]
    public class XMLOperationBase
    {
        /// <summary>
        /// 操作编号
        /// </summary>
        [XmlAttribute("OpId", DataType = "int")]
        public int OpId { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }


        [XmlElement("OperationGuide")]
        public string OperationGuide { get; set; }

        /// <summary>
        /// 本次操作的依赖关系
        /// </summary>
        [XmlArray("Conditions")]
        [XmlArrayItem("Condition")]
        public List<int> Conditions { get; set; }

        /// <summary>
        /// 本次操作结束后播放的动画（可以是单个动画或者多个动画）
        /// </summary>
        [XmlArray("ActionIds")]
        [XmlArrayItem("ActionId")]
        public List<int> ActionIds { get; set; }



        [XmlElement("OperationType")]
        public string OperationType { get; set; }
        /// <summary>
        /// 本次操作扣分
        /// </summary>
        [XmlElement("Score")]
        public int Score { get; set; }


        /// <summary>
        /// 本次操作扣分内容
        /// </summary>
        [XmlElement("LogContent")]
        public string LogContent { get; set; }
        /// <summary>
        /// 该操作是否无关（默认有关）
        /// </summary>
        [XmlElement("CanIgnore")]
        public bool CanIgnore { get; set; }
        /// <summary>
        /// 动画集合
        /// </summary>
        private List<XMLActionBase> Actions = new List<XMLActionBase>();
        /// <summary>
        /// 操作完成状况(指操作完成且动画播完)
        /// </summary>
        [SerializeField]
        private bool _Finished = false;
        [XmlIgnore]
        public bool Finished
        {
            get
            {
                return _Finished;
            }
            private set
            {
                _Finished = value;
            }
        }

        /// <summary>
        /// 关闭当前操作检测
        /// </summary>
        private bool closeOperationcheck = false;

        private bool isStart = false;
        /// <summary>
        /// 满足条件
        /// </summary>
        private bool meetConditions = false;
        [XmlIgnore]
        public bool MeetConditions
        {
            get
            {
                return meetConditions;
            }
            private set
            {
                meetConditions = value;

            }
        }


        /// <summary>
        /// 检测前置操作是否完成
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private bool CheckCondition(XMLStandardStepProcess process)
        {
            //前置operation为空，前置已完成
            if (Conditions.Count == 0)
            {
                meetConditions = true;
                return true;
            }
            //获取所有的前置operation
            List<XMLOperationBase> ops = new List<XMLOperationBase>();
            for (int i = 0, imax = Conditions.Count; i < imax; i++)
            {
                if (process.GetOperations().Exists(p => p.OpId.Equals(Conditions[i])))
                {
                    ops.Add(process.GetOperations().Find(p => p.OpId.Equals(Conditions[i])));
                }
                else
                {
                    Debug.LogError("当前操作:"+ OpId + "->不存在前置条件操作ID:" + Conditions[i]);
                }
            }
            //存在未完成的前置
            if ((ops.Exists(p => p.Finished.Equals(false))))
            {
                meetConditions = false;
                return false;
            }
            else//前置已完成
            {
                meetConditions = true;
                return true;
            }
        }

        /// <summary>
        /// 跳步
        /// </summary>
        /// <param name="process">流程</param>
        public void SkipStep(XMLStandardStepProcess process)
        {
            if (meetConditions || CheckCondition(process))
            {
                OnSkip(process);
            }
        }

        /// <summary>
        /// 操作
        /// </summary>
        /// <param name="process"></param>
        /// <param name="unityAction"></param>
        public void Execute(XMLProcess xMLProcessLogic, XMLStandardStepProcess process, UnityAction<XMLOperationBase> unityAction)
        {
            //前置操作全部完成后执行
            if (meetConditions || CheckCondition(process))
            {
                //操作正在执行
                if (isStart)  
                {
                    if (closeOperationcheck)//关闭当前操作检测，开启Action
                    {
                        if (CheckActionState())//所有action完成
                        {
                            OnEnd();
                            Finished = true;
                            unityAction(this);//结束一个operation回调一次
                        }
                        else//action未完成
                        {
                            for (int i = Actions.Count - 1; i >= 0; i--)
                            {
                                XMLActionBase action = Actions[i];
                                if (!action.isFinished)
                                {
                                    action.Execute(xMLProcessLogic);
                                }
                            }
                        }
                    }
                    else
                    {
                        //判断operation自身逻辑是否结束
                        if (OnUpdate(process))
                        {
                            closeOperationcheck = true;
                        }
                    }

                }
                else
                {
                    //开始首次执行，重置所有Action  isStart=true
                    OnStart(xMLProcessLogic);
                }
            }
        }

        //public virtual void Revert(XMLStandardStepProcess process)
        //{
        //    List<XMLActionBase> allActions = GetAllActions();
        //    for (int i = allActions.Count - 1; i >= 0; i--)
        //    {
        //        if (allActions[i].isRecorded)
        //            allActions[i].Revert();
        //    }
        //}

        //private List<XMLActionBase> GetAllActions()
        //{
        //    List<XMLActionBase> list = new List<XMLActionBase>();
        //    list.AddRange(Actions);
        //    foreach (var action in Actions)
        //    {
        //        list.AddRange(GetAllActionsRecursion(action));
        //    }
        //    return list;
        //}

        //private List<XMLActionBase> GetAllActionsRecursion(XMLActionBase action)
        //{
        //    List<XMLActionBase> list = new List<XMLActionBase>();
        //    List<XMLActionBase> nextActions = FrameEntrance.Instance.Process.GetActions(action.ActionIds);
        //    list.AddRange(nextActions);
        //    foreach (var childAction in nextActions)
        //    {
        //        list.AddRange(GetAllActionsRecursion(childAction));
        //    }
        //    return list;
        //}

        //public virtual void Jump(XMLStandardStepProcess process)
        //{
        //    foreach (var action in Actions)
        //    {
        //        JumpActionRecursion(action);
        //    }
        //    Finished = true;
        //}

        //private void JumpActionRecursion(XMLActionBase action)
        //{
        //    action.Record();
        //    action.FinishImmediately();
        //    List<XMLActionBase> nextActions = FrameEntrance.Instance.Process.GetActions(action.ActionIds);
        //    foreach (var nextAction in nextActions)
        //    {
        //        JumpActionRecursion(nextAction);
        //    }
        //}

        /// <summary>
        /// 重置参数
        /// </summary>
        public virtual void ResetParameters()
        {
            isStart = false;
            Finished = false;
            closeOperationcheck = false;
            meetConditions = false;
        }

        /// <summary>
        /// 检查所有动画状态
        /// </summary>
        /// <returns></returns>
        private bool CheckActionState()
        {
            if (Actions.Exists(p => p.isFinished.Equals(false)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnOperationGuide()
        {
            //if (FrameEntrance.Instance.DataNode.GetData<VarString>("Model") != "考核" && OperationType != "无提示" && !string.IsNullOrEmpty(OperationGuide))
            //    FrameEntrance.Instance.Event.DispatchCoreEvent(new CoreEvent(CoreEventId.OperationGuide, new OpPanelparam("", new string[] { OperationGuide, OperationType })));//发送一个消息，提示本操作步骤

        }


        /// <summary>
        /// operation第一次开始执行/重置所有相关Action
        /// </summary>
        /// <param name="xMLProcessLogic"></param>
        public virtual void OnStart(XMLProcess xMLProcessLogic)
        {
            Actions = xMLProcessLogic.GetActions(ActionIds);
            for (int i = 0, imax = Actions.Count; i < imax; i++)
            {
                Actions[i].ResetParameters();
            }
            isStart = true;
        }

        /// <summary>
        /// operation自身的逻辑执行
        /// </summary>
        /// <param name="process"></param>
        /// <returns>返回true则自身逻辑结束</returns>
        public virtual bool OnUpdate(XMLStandardStepProcess process)
        {
            return false;
        }
        /// <summary>
        /// operation自身逻辑结束，且关联action也全部结束后执行
        /// </summary>
        public virtual void OnEnd()
        {

        }
        /// <summary>
        /// 中途跳出步骤（清理操作状态）
        /// </summary>
        public virtual void OnSkip(XMLStandardStepProcess process) { }

    }
}