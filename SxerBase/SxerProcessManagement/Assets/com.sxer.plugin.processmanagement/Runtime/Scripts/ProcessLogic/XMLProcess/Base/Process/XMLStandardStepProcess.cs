using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
namespace Sxer.Frame.Plugin.ProcessManagement.XMLProcess
{
    [System.Serializable]
    [XmlType("XMLStandardStepProcess")]
    public class XMLStandardStepProcess : XMLProcessBase
    {
        [XmlArray("OperationIds")]
        [XmlArrayItem("OperationId")]
        public List<int> OperationIds { get; set; }

        [XmlElement("UseRecord")]
        public bool UseRecord { get; set; }

        /// <summary>
        /// 当前流程所有的操作
        /// </summary>
        private List<XMLOperationBase> Operations = new List<XMLOperationBase>();



        private XMLProcess m_XMLProcessLogic; 
        public override void DoFirstEntering(XMLProcess xMLProcessLogic)
        {
            base.DoFirstEntering(xMLProcessLogic);
            m_XMLProcessLogic = xMLProcessLogic;
            Operations = m_XMLProcessLogic.GetOperations(OperationIds);
        }

        public override void OnUpdate()
        {
            for (int i = 0, imax = Operations.Count; i < imax; i++)
            {
                if (!Operations[i].Finished)
                {
                    Operations[i].Execute(m_XMLProcessLogic, this, OnOperationFinished);
                }
            }
        }

        public override void DoBeforeLeaving()
        {
            base.DoBeforeLeaving();
            for (int i = 0, imax = Operations.Count; i < imax; i++)
            {
                Operations[i].ResetParameters();
            }
        }


       
        /// <summary>
        /// 获取所有操作
        /// </summary>
        /// <returns></returns>
        public List<XMLOperationBase> GetOperations()
        {
            return Operations;
        }

        /// <summary>
        /// 每个operation完成后执行一次
        /// </summary>
        /// <param name="op"></param>
        private void OnOperationFinished(XMLOperationBase op)
        {
     //       FrameEntrance.Instance.Event.DispatchCoreEvent(new CoreEvent(CoreEventId.OnOperationFinished, op.opId));
            CheckProcessTransitions();
        }
        /// <summary>
        /// 检查流程是否可以跳转
        /// </summary>
        private void CheckProcessTransitions()
        {
            //当前流程所有operation执行完毕
            if (!Operations.Exists(p => p.Finished.Equals(false)))
            {
                m_XMLProcessLogic.ChangeProcess(NextProcessName);
            }
        }
    
        public override void SkipProcess()
        {
            base.SkipProcess();
            for (int i = 0, imax = Operations.Count; i < imax; i++)
            {
                if (!Operations[i].Finished)
                {
                    Operations[i].SkipStep(this);
                }
            }
        }

      

        /// <summary>
        /// 返回-1 op1是op2的父节点， 返回1 op2是op1的父节点， 返回0 op1等于op2
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
        private int OperationCompare(XMLOperationBase op1, XMLOperationBase op2)
        {
            if (op1 == null || op2 == null)
            {
                throw new System.NullReferenceException();
            }
            if (op1.OpId == op2.OpId)
            {
                return 0;
            }
            XMLOperationBase parent = GetFirstParentOperation(op2);
            while (parent != null)
            {
                if (parent.OpId == op1.OpId)
                    return -1;
                parent = GetFirstParentOperation(parent);
            }
            parent = GetFirstParentOperation(op1);
            while (parent != null)
            {
                if (parent.OpId == op2.OpId)
                    return 1;
                parent = GetFirstParentOperation(parent);
            }
            return -2;
        }

        private XMLOperationBase GetFirstParentOperation(XMLOperationBase operation)
        {
            if (operation.Conditions.Count == 0)
                return null;

            List<XMLOperationBase> ops = new List<XMLOperationBase>();
            List<int> conditions = operation.Conditions;
            return GetOperations().Find(p => p.OpId.Equals(conditions[0]));
        }

        private XMLOperationBase GetCurOperation()
        {
            foreach (var v in Operations)
            {
                XMLOperationBase parentOperation = GetFirstParentOperation(v);
                if (!v.Finished && (parentOperation == null || parentOperation.Finished))
                {
                    return v;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 自定义过渡类，每个Step可以包含多个过渡
    /// </summary>

    public class StandardStepTransition
    {
        /// <summary>
        /// 条件操作id
        /// </summary>
        public List<int> opIds = new List<int>();
        /// <summary>
        /// 目标流程名
        /// </summary>
        public string processName;

    }

}





