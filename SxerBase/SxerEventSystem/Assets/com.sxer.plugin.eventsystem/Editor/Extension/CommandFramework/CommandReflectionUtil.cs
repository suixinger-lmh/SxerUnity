#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sxer.Plugin.EventSystem.Cmd.Editor
{
    public static class CommandReflectionUtil
    {
        /// <summary>
        /// 获取所有的“分类”（继承自 CommandHandler 的 abstract 类）
        /// </summary>
        public static List<Type> GetCategories()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsTargetAssembly(a.GetName().Name))
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsAbstract && t.IsClass && t.IsSubclassOf(typeof(CommandHandler)) && !t.IsGenericType)
                .ToList();
        }

        /// <summary>
        /// 获取某个分类下的所有“具体指令实现”
        /// </summary>
        public static List<Type> GetConcreteCommands(Type categoryType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsTargetAssembly(a.GetName().Name))
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract && categoryType.IsAssignableFrom(t))
                .ToList();
        }

        /// <summary>
        /// 过滤无效的程序集，显著优化编辑器下的反射检索效率
        /// </summary>
        private static bool IsTargetAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName)) return false;
            return !assemblyName.StartsWith("System") &&
                   !assemblyName.StartsWith("Unity") &&
                   !assemblyName.StartsWith("Mono") &&
                   !assemblyName.StartsWith("Microsoft") &&
                   !assemblyName.StartsWith("nunit") &&
                   assemblyName != "mscorlib";
        }
    }
}
#endif