using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Sxer.Plugin.ProcessManagement.XMLProcess
{
    [System.Serializable]
    [XmlType("XMLProcessBase")]
    public class XMLProcessBase
    {
        /// <summary>
        /// 操作描述(配置用于调试)
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlAttribute("ProcessName", DataType = "string")]
        public string ProcessName { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        [XmlElement("MyTask")]
        public string MyTask { get; set; }

        [XmlAttribute("NextProcessName", DataType = "string")]
        public string NextProcessName { get; set; }


        /// <summary>
        /// 第一次进入流程执行
        /// </summary>
        //public virtual void DoFirstEntering()
        //{
        //   // Debug.Log(" DoFirstEntering >>>>  " + ProcessName);
        //}

        public virtual void DoFirstEntering(XMLProcess xMLProcessLogic)
        {
            // Debug.Log(" DoFirstEntering >>>>  " + ProcessName);
        }
        /// <summary>
        /// 离开流程前执行
        /// </summary>
        public virtual void DoBeforeLeaving()
        {
            //FrameEntrance.Instance.StopAllCoroutines();
            //DG.Tweening.DOTween.KillAll();
            //vp_Timer.CancelAll();
        }
        /// <summary>
        /// 流程循环
        /// </summary>
        public virtual void OnUpdate()
        {

        }
        /// <summary>
        /// 重置参数
        /// </summary>
        public virtual void ResetParameters() { }
        public virtual void SkipProcess() { }


    }
}