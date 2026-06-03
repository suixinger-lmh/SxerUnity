using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace Sxer.Plugin.EventSystem.Cmd
{
    public class CommandDispatcher
    {
        private readonly Dictionary<string, CommandHandler> _handlerMap = new Dictionary<string, CommandHandler>();
        private readonly string _categoryName;

        public CommandDispatcher(string categoryName, List<CommandHandler> handlers)
        {
            _categoryName = categoryName;

            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    if (handler == null || string.IsNullOrEmpty(handler.CommandId)) continue;

                    if (!_handlerMap.ContainsKey(handler.CommandId))
                    {
                        _handlerMap.Add(handler.CommandId, handler);
                    }
                    else
                    {
                        Debug.LogWarning($"[Command] 在分类 {_categoryName} 中发现重复的指令ID: {handler.CommandId}");
                    }
                }
            }
        }

        public void Dispatch(string cmdId, CommandHandlerArg arg)
        {
            if (_handlerMap.TryGetValue(cmdId, out var handler))
            {
                handler.ExecuteRaw(arg);
            }
        }
    }
}