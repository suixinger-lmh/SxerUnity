using UnityEngine;

namespace Sxer.Frame
{
    /// <summary>
    /// 全局单例组件基类（框架启动时加载，全局唯一）
    /// </summary>
    public abstract class SxerSingletonComponent<T> : SxerComponentBase where T : SxerSingletonComponent<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = (T)this;
                LifeType = ComponentLifeType.GlobalSingleton; // 强制设为单例类型
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    [SxerComponent("动态组件基类", ComponentLifeType.DynamicInstance, ComponentInitType.None,0)]
    /// <summary>
    /// 动态多实例组件基类（运行时可动态添加、销毁，允许多个）
    /// </summary>
    public abstract class SxerDynamicComponent : SxerComponentBase
    {
        protected virtual void Awake()
        {
            LifeType = ComponentLifeType.DynamicInstance; // 强制设为动态类型
        }
    }
}