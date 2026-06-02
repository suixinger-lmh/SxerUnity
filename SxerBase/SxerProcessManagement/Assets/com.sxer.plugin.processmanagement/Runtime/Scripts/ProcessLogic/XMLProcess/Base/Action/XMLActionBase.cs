using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Sxer.Frame.Plugin.ProcessManagement.XMLProcess
{
    [XmlType("ActionBase")]
    public class XMLActionBase
    {
        [XmlAttribute("AcId", DataType = "int")]
        public int AcId { get; set; }
     

        /// <summary>
        /// 动画描述(配置用于调试)
        /// </summary>
        [XmlElement("description")]
        public string description { get; set; }

        /// <summary>
        /// 延迟播放该动画的时间
        /// </summary>
        [XmlElement("Delay")]
        public float Delay { get; set; }


        [XmlArray("ActionIds")]
        [XmlArrayItem("ActionId")]
        public List<int> ActionIds { get; set; }

        /// <summary>
        /// 后续动画
        /// </summary>
        private List<XMLActionBase> Actions;
        protected bool _isFinished = false;
        [XmlIgnore]
        public bool isFinished
        {
            get
            {
                return _isFinished;
            }
            private set
            {
                _isFinished = value;

            }
        }
        protected bool currentActionFinished = false;
        protected bool IsStart = false;
        protected bool isDelaying = false;

        protected Coroutine m_DelayStartCo = null;

        protected Hashtable m_RecordData = new Hashtable();
        public bool isRecorded { get { return m_IsRecorded; } }

        protected bool m_IsRecorded = false;

        [XmlIgnore]
        public bool Running
        {
            get
            {
                return (IsStart && !_isFinished);

            }

        }
        /// <summary>
        /// 重置参数
        /// </summary>
        public virtual void ResetParameters()
        {
            currentActionFinished = false;
            IsStart = false;
            isDelaying = false;
            _isFinished = false;
        }

        /// <summary>
        /// Action自身逻辑
        /// </summary>
        public void Execute(XMLProcess xMLProcessLogic)
        {
            if (!IsStart)//开始执行
            {
                //此Action关联的action重置
                Actions = xMLProcessLogic.GetActions(ActionIds);
                for (int i = 0, imax = Actions.Count; i < imax; i++)
                {
                    Actions[i].ResetParameters();
                }
                if (!isDelaying)
                {
                    //所有动画都通过协成执行
                    m_DelayStartCo = xMLProcessLogic.StartCoroutine(DelayStart());
                    isDelaying = true;
                }
            }
            else//正在执行
            {
                if (currentActionFinished)
                {
                    if (CheckActionState())//如果action全部执行完，当前操作完成
                    {
#if UNITY_EDITOR
                        Debug.Log(this.AcId + "   >>>>   " + this.description + " >>> 动画   结束");
#endif
                        isFinished = true;
                        OnEnd();
                    }
                    else
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
                    if (OnUpdate())
                    {
                        currentActionFinished = true;
                        if (Actions.Count == 0)
                        {
                            isFinished = true;
#if UNITY_EDITOR
                            Debug.Log(this.AcId + "   >>>>   " + this.description + " >>> 动画   结束 ");
#endif
                            OnEnd();
                        }
                    }
                }

            }
        }
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

        //public virtual void FinishImmediately()
        //{
        //    if (!IsStart)
        //    {
        //        if (m_DelayStartCo != null)
        //            FrameEntrance.Instance.StopCoroutine(m_DelayStartCo);
        //        IsStart = true;
        //        isDelaying = true;
        //        OnStart();
        //    }
        //    currentActionFinished = true;
        //    isFinished = true;
        //}

        //public void Record()
        //{
        //    if (!m_IsRecorded)
        //    {
        //        _Record();
        //        m_IsRecorded = true;
        //    }
        //}

        //protected virtual void _Record()
        //{
        //}

        //public virtual void Revert()
        //{
        //    FinishImmediately();
        //    isDelaying = false;
        //    IsStart = false;
        //    currentActionFinished = false;
        //    _isFinished = false;
        //}

        
        IEnumerator DelayStart()
        {
            yield return new WaitForSeconds(Delay);
          //  Record();
            OnStart();
            IsStart = true;

        }
        /// <summary>
        /// 动画首次开始执行
        /// </summary>
        public virtual void OnStart()
        {

        }
        /// <summary>
        /// 动画循环
        /// </summary>
        /// <returns></returns>
        public virtual bool OnUpdate()
        {
            return false;
        }

        /// <summary>
        /// 动画结束
        /// </summary>
        public virtual void OnEnd()
        {
        }
    }
}