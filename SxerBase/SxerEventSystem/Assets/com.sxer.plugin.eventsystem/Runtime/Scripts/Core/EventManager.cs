using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Sxer.Plugin.EventSystem
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private readonly CoreEventDispatcher m_EventDispatcher = new CoreEventDispatcher();
        private readonly ConcurrentQueue<IDeferredEvent> m_eventQueue = new ConcurrentQueue<IDeferredEvent>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddEventListener<T>(Action<T> handler) where T : CoreEvent<T>
        {
            m_EventDispatcher.AddEventListener(handler);
        }

        public void RemoveEventListener<T>(Action<T> handler) where T : CoreEvent<T>
        {
            m_EventDispatcher.RemoveEventListener(handler);
        }

        public void ClearEventListener<T>() where T : CoreEvent<T>
        {
            m_EventDispatcher.ClearEventListener<T>();
        }

        public void ClearAllEventListener()
        {
            m_EventDispatcher.ClearAllEventListener();
        }

        /// <summary>
        /// 立即分发事件（非线程安全，必须在主线程执行）
        /// </summary>
        public void DispatchCoreEventImmediately<T>(T evt) where T : CoreEvent<T>
        {
            m_EventDispatcher.DispatchCoreEvent(evt);
        }

        /// <summary>
        /// 延迟到下一帧分发（线程安全，可在子线程安全调用，内部已做零GC池化处理）
        /// </summary>
        public void DispatchCoreEvent<T>(T evt) where T : CoreEvent<T>
        {
            var deferred = DeferredEvent<T>.Pool.Get();
            deferred.EventData = evt;
            m_eventQueue.Enqueue(deferred);
        }

        private void Update()
        {
            while (m_eventQueue.Count > 0 && m_eventQueue.TryDequeue(out var deferredEvent))
            {
                if (deferredEvent != null)
                {
                    deferredEvent.Execute(m_EventDispatcher);
                    deferredEvent.Release();
                }
            }
        }

        #region 内部辅助结构（零GC延迟派发设计）

        private interface IDeferredEvent
        {
            void Execute(CoreEventDispatcher dispatcher);
            void Release();
        }

        private class DeferredEvent<T> : IDeferredEvent where T : CoreEvent<T>
        {
            public T EventData;

            public void Execute(CoreEventDispatcher dispatcher)
            {
                dispatcher.DispatchCoreEvent(EventData);
            }

            public void Release()
            {
                EventData = null;
                Pool.Release(this);
            }

            public static readonly SimpleObjectPool<DeferredEvent<T>> Pool = new SimpleObjectPool<DeferredEvent<T>>();
        }

        private class SimpleObjectPool<T> where T : class, new()
        {
            private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

            public T Get() => _queue.TryDequeue(out var item) ? item : new T();
            public void Release(T item) => _queue.Enqueue(item);
        }

        #endregion
    }
}