using UnityEngine;
using System.Collections.Generic;
using System;

namespace Sxer.Plugin.EventSystem
{
    public class CoreEventDispatcher
    {
        private readonly Dictionary<string, Delegate> mEventHandlerPool = new Dictionary<string, Delegate>();
        private readonly object m_Lock = new object();

        /* 消息注册函数 */
        public void AddEventListener<T>(Action<T> handler) where T : CoreEvent<T>
        {
            if (handler == null) return;
            string id = CoreEvent<T>.EventID;

            lock (m_Lock)
            {
                if (mEventHandlerPool.TryGetValue(id, out var existing))
                {
                    mEventHandlerPool[id] = Delegate.Combine(existing, handler);
                }
                else
                {
                    mEventHandlerPool[id] = handler;
                }
            }
        }

        /* 消息移除函数 */
        public void RemoveEventListener<T>(Action<T> handler) where T : CoreEvent<T>
        {
            if (handler == null) return;
            string id = CoreEvent<T>.EventID;

            lock (m_Lock)
            {
                if (mEventHandlerPool.TryGetValue(id, out var existing))
                {
                    var updated = Delegate.Remove(existing, handler);
                    if (updated == null)
                        mEventHandlerPool.Remove(id);
                    else
                        mEventHandlerPool[id] = updated;
                }
            }
        }

        /* 清除特定消息监听 */
        public void ClearEventListener<T>() where T : CoreEvent<T>
        {
            string id = CoreEvent<T>.EventID;
            lock (m_Lock)
            {
                mEventHandlerPool.Remove(id);
            }
        }

        /* 清除所有消息监听 */
        public void ClearAllEventListener()
        {
            lock (m_Lock)
            {
                mEventHandlerPool.Clear();
            }
        }

        /* 消息分发函数 */
        public void DispatchCoreEvent<T>(T evt) where T : CoreEvent<T>
        {
            if (evt == null) return;
            string id = CoreEvent<T>.EventID;
            Delegate d = null;

            lock (m_Lock)
            {
                mEventHandlerPool.TryGetValue(id, out d);
            }

            if (d is Action<T> callback)
            {
                try
                {
                    callback.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError($"事件异常：{id}\n{e}");
                }
            }
        }
    }
}