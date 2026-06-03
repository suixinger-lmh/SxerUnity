
using System;
using UnityEngine;

namespace Sxer.Plugin.EventSystem.Cmd
{
    

    /// <summary>
    /// 指令参数类型
    /// </summary>
    public abstract class CommandHandlerArg {}

    [Serializable]
    /// <summary>
    /// 指令统一基类
    /// </summary>
    public abstract class CommandHandler {

        /// <summary>
        /// 指令ID (由子类强制实现，替代原本在面板上绑定的方式)
        /// </summary>
        public abstract string CommandId { get; }
        /// <summary>
        /// 指令描述
        /// </summary>
        public virtual string CmdDesc => "未命名指令";

        // 提供给 Manager 调用的统一接口
        public abstract void ExecuteRaw(CommandHandlerArg arg);
    }
    [Serializable]
    /// <summary>
    /// 指令处理基类 (泛型 T 代表传入参数的类型，比如 string, byte[], 或者一个自定义类)
    /// </summary>
    public abstract class CommandHandlerBase<T> : CommandHandler where T: CommandHandlerArg
    {

        // 隐藏底层的转型逻辑，开发者不需要关心
        public override void ExecuteRaw(CommandHandlerArg arg)
        {
            // 安全的向下转型 (Downcasting)
            if (arg is T typedArg)
            {
                Execute(typedArg);
            }
            else
            {
                Debug.LogError($"[Command] {CommandId}指令参数类型不匹配！期望 {typeof(T).Name}，实际收到 {arg?.GetType().Name}");
            }
        }

        public virtual void Execute(T arg)
        {   
            ExecuteAction(arg);
            Debug.Log($"[Command] {CommandId} ({CmdDesc}) 执行完毕。");
        }

       

        // ================== 生命周期方法，供子类重写 ==================

        /// <summary> 真正的执行逻辑 </summary>
        protected abstract void ExecuteAction(T arg);

        

        
    }
}