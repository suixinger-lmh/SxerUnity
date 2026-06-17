using Sxer.Plugin.ProcessManagement.XMLProcess.Tool;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Sxer.Plugin.ProcessManagement.XMLProcess
{
    /// <summary>
    /// 配表实现流程
    /// </summary>
    public class XMLProcess : ProcessBase
    {
        [Header("流程配置")]
        [SerializeField]
        private TextAsset processXML;
        [Header("操作集合")]
        [SerializeField]
        private TextAsset operationXML;
        [Header("表现集合")]
        [SerializeField]
        private TextAsset actionXML;


        private XMLProcessBase m_EntranceProcess = null;
        public XMLProcessBase GetEntranceProcess()
        {
            return m_EntranceProcess;
        }
        protected override void OnInit()
        {
            base.OnInit();
            //读取配置数据
            ReadXmlData();
        }


        protected override void OnStart()
        {
            base.OnStart();
            if (m_EntranceProcess != null)
            {
                runProcess = true;

                m_EntranceProcess.DoFirstEntering(this);
            }
            else
            {
                Debug.LogError("无流程！");
            }
        }


        protected override void OnPause()
        {
            base.OnPause();
            runProcess = false;
        }

        protected override void OnStop()
        {
            base.OnStop();
            runProcess = false;
        }
        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (runProcess)
            {
                if (m_EntranceProcess != null)
                {
                    m_EntranceProcess.OnUpdate();
                }
            }

        }




        private bool runProcess = false;


        List<XMLProcessBase> processes = new List<XMLProcessBase>();
        List<XMLOperationBase> operations = new List<XMLOperationBase>();
        List<XMLActionBase> actions = new List<XMLActionBase>();
        List<Type> ProcessTypes = new List<Type>();
        List<Type> OperationTypes = new List<Type>();
        List<Type> ActionTypes = new List<Type>();
        private void ReadXmlData()
        {
            //反射获取所有xml反序列化的子类
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(XMLProcessBase)))
                {
                    ProcessTypes.Add(item);
                    continue;
                } 
                if (item.IsSubclassOf(typeof(XMLOperationBase)))
                {
                    OperationTypes.Add(item);
                    continue;
                }
                if (item.IsSubclassOf(typeof(XMLActionBase)))
                {
                    ActionTypes.Add(item);
                    continue;
                }
            }

            //反序列化
            if (processXML != null)
            {
                ProcessXmlSerializer xml = XMLHelper.XMLDeserialize<ProcessXmlSerializer>(processXML.bytes, ProcessTypes.ToArray());
                processes = xml.process;
            }

            if (operationXML != null)
            {
                OperationXmlSerializer xml = XMLHelper.XMLDeserialize<OperationXmlSerializer>(operationXML.bytes, OperationTypes.ToArray());
                operations = xml.operation;

            }
            if (actionXML != null)
            {
                ActionXmlSerializer xml = XMLHelper.XMLDeserialize<ActionXmlSerializer>(actionXML.bytes, ActionTypes.ToArray());
                actions = xml.action;
            }
            if (processes.Count > 0)
                m_EntranceProcess = processes[0];//配表默认从第一个开始执行
        }


        public List<XMLProcessBase> GetProcessBases()
        {
            if (processes.Count > 0)
            {
                return processes;
            }
            else
            {
                Debug.LogError("流程为空");
                return null;
            }
        }
        public List<XMLOperationBase> GetOperationBases()
        {
            if (operations.Count > 0)
            {
                return operations;
            }
            else
            {
                Debug.LogError("操作为空");
                return null;
            }
        }
        public List<XMLActionBase> GetActionBases()
        {
            if (actions.Count > 0)
            {
                return actions;
            }
            else
            {
                Debug.LogError("动画为空");
                return null;
            }
        }

       

        /// <summary>
        /// 获取操作集合
        /// </summary>
        /// <param name="opids">id列表</param>
        /// <returns></returns>
        public List<XMLOperationBase> GetOperations(List<int> opids)
        {

            {
                List<XMLOperationBase> Operations = new List<XMLOperationBase>();
                for (int i = 0, imax = opids.Count; i < imax; i++)
                {
                    if (operations.Exists(p => p.OpId.Equals(opids[i])))
                    {
                        XMLOperationBase item = operations.Find(p => p.OpId.Equals(opids[i]));
                        Operations.Add(item);
                    }
                    else
                    {
                        Debug.LogError("不存在的操作 opid >>>>  " + opids[i]);
                    }
                }
                return Operations;
            }

        }
        /// <summary>
        /// 获取动画集合
        /// </summary>
        /// <param name="acids">id列表</param>
        /// <returns></returns>
        public List<XMLActionBase> GetActions(List<int> acids)
        {

            {
                List<XMLActionBase> Actions = new List<XMLActionBase>();
                for (int i = 0, imax = acids.Count; i < imax; i++)
                {
                    if (actions.Exists(p => p.AcId.Equals(acids[i])))
                    {
                        XMLActionBase item = actions.Find(p => p.AcId.Equals(acids[i]));
                        Actions.Add(item);
                    }
                    else
                    {
                        Debug.LogError(" 不存在的操作 acId >>>>  " + acids[i]);
                    }
                }
                return Actions;
            }

        }

        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(XMLOperationBase operation)
        {
            if (operation == null)
            {
                Debug.LogError("空的operation");
                return;
            }

            if (operations.Exists(p => p.OpId.Equals(operation.OpId)))
            {
                Debug.LogError("重复的opId");
                return;
            }

            operations.Add(operation);
        }
        /// <summary>
        /// 添加动画
        /// </summary>
        public void AddAction(XMLActionBase action)
        {
            if (action == null)
            {
                Debug.LogError("空的action");
                return;
            }

            if (actions.Exists(p => p.AcId.Equals(action.AcId)))
            {
                Debug.LogError("重复的acId");
                return;
            }

            actions.Add(action);
        }

        public XMLProcessBase GetProcessByName(string processName)
        {
            return processes.Find(p => p.ProcessName.Equals(processName));
        }
        public void ChangeProcess(string processName)
        {
            XMLProcessBase process = GetProcessByName(processName);
            if (process == null)
            {
                Debug.LogError("不存在的processName:"+ processName);
            }
            ChangeProcess(process);
        }

        public void ChangeProcess(XMLProcessBase process)
        {
            m_EntranceProcess.DoBeforeLeaving();
            if (process == null)
            {
                Debug.LogError("空流程!");
                return;
            }
            if (!processes.Contains(process))
            {
                Debug.LogError("不在列表中的process!");
                return;
            }
            m_EntranceProcess = process;
            m_EntranceProcess.DoFirstEntering(this);
        }
    }
}