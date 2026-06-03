using System.Collections.Generic;
using UnityEngine;


namespace Sxer.Plugin.EventSystem.Cmd
{
    public class CMDManager : MonoBehaviour
    {
        // 存储当前 Manager 管理的分类类型名称 (如 "Stored3D.Command.JsCommandHandler")
        [HideInInspector]
        public string managedCategoryTypeName;

        // 该分类下的所有指令实例
        [SerializeReference]
        public List<CommandHandler> handlers = new List<CommandHandler>();

        // 运行时的分发器
        private CommandDispatcher _dispatcher;

        void Start()
        {
            // 初始化分发器
            _dispatcher = new CommandDispatcher(managedCategoryTypeName, handlers);

            Debug.Log(managedCategoryTypeName);
            Debug.Log(handlers.Count);

            // 订阅全局指令事件
            // 注册到全局单例事件管理器
            if (EventManager.Instance != null)
            {
                EventManager.Instance.AddEventListener<CMDEvent>(OnCommandReceived);
            }
        }

        void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveEventListener<CMDEvent>(OnCommandReceived);
            }
        }

        private void OnCommandReceived(CMDEvent ce)
        {
            if (ce == null || ce.commandHandlerArg == null) return;

            // 让分发器去处理
            _dispatcher.Dispatch(ce.cmdID, ce.commandHandlerArg);
        }
    }
}