using System;

namespace Sxer.Frame
{
    public enum FrameState { None, Initializing, Running, DisposeIng, Destroyed }
    public enum ComponentState { UnInit, InitIng, Inited, DisposeIng, ReloadIng, Destroyed }
    public enum ComponentInitType { None, Sync, Async }

    /// <summary>
    /// 组件类型（框架管理策略）
    /// </summary>
    public enum ComponentLifeType { GlobalSingleton, DynamicInstance }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SxerComponentAttribute : Attribute
    {
        public ComponentLifeType LifeType { get; }
        public ComponentInitType InitType { get; }
        public int InitPriority { get; }
        public string Description { get; }

        public SxerComponentAttribute(string description, ComponentLifeType lifeType, ComponentInitType initType, int initPriority = 100)
        {
            Description = description;
            LifeType = lifeType;
            InitType = initType;
            InitPriority = initPriority;
        }
    }
}