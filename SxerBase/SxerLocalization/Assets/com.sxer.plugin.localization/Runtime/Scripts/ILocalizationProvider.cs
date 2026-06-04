using System;
using System.Collections.Generic;

namespace Sxer.Plugin.Localization
{
    public interface ILocalizationProvider
    {
        List<LanguageSettings> GetAllLanguage();

    }


    /// <summary>
    /// 本地化提供者优先级属性
    /// 标记在实现 ILocalizationProvider 的类上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class LocalizationProviderAttribute : Attribute
    {
        /// <summary>
        /// 优先级（数值越大，越优先被选中）
        /// </summary>
        public int Priority { get; }

        public LocalizationProviderAttribute(int priority)
        {
            Priority = priority;
        }
    }
}