using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sxer.Plugin.Localization
{
    public static class LocalizationManager
    {
        public static bool Initialized { get; private set; }
        public static LanguageSettings CurrentLanguage { get; private set; }


        public static ILocalizationProvider Provider { get; private set; }

        //修改语言后的回调执行
        public static event Action<SystemLanguage> OnSetLanguage;


        private static List<LanguageSettings> languages;

        private static LanguageSettings defaultLanguage;


        private static void Initialize()
        {
            if (Initialized) return;

            // 第一步：反射扫描所有合法的本地化提供者
            var providerType = FindBestLocalizationProvider();
            if (providerType == null)
            {
                Debug.LogError("[Localization] 未找到任何标记 [LocalizationProvider] 的实现类！");
                return;
            }

            try
            {
                // 第二步：实例化最优提供者
                Provider = (ILocalizationProvider)Activator.CreateInstance(providerType);

                // 第三步：执行获取本地化数据方法
                languages = Provider.GetAllLanguage();

                // 第四步：读取本地保存的语言（默认系统语言）
                var saveLanguage = (SystemLanguage)PlayerPrefs.GetInt(
                    "language",
                    (int)Application.systemLanguage
                );

                // 第五步：设置语言
                SetLanguage(saveLanguage);

                Initialized = true;
                Debug.Log($"[Localization] 初始化完成，使用提供者：{providerType.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Localization] 初始化失败：{e.Message}");
            }
        }

        /// <summary>
        /// 反射查找【优先级最高】的本地化提供者类型
        /// </summary>
        private static Type FindBestLocalizationProvider()
        {
            var validProviders = new List<(Type type, int priority)>();

            // 遍历当前应用域所有程序集（优化：可只遍历游戏程序集，提升性能）
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
              
                // 获取程序集中所有类
                var types = assembly.GetTypes().Where(t =>
                    t.IsClass &&          // 必须是类
                    !t.IsAbstract &&      // 不能是抽象类
                    !t.IsGenericType &&   // 不能是泛型类
                    typeof(ILocalizationProvider).IsAssignableFrom(t) && // 实现接口
                    t.GetCustomAttribute<LocalizationProviderAttribute>() != null // 带优先级属性
                );

                // 收集符合条件的类 + 优先级
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<LocalizationProviderAttribute>();
                    validProviders.Add((type, attr.Priority));
                }
            }
            catch
            {
                // 跳过加载异常的程序集
            }
            //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    try
            //    {
            //        // 跳过系统/Unity原生程序集（可选优化）
            //        if (assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("System"))
            //            continue;

            //        // 获取程序集中所有类
            //        var types = assembly.GetTypes().Where(t =>
            //            t.IsClass &&          // 必须是类
            //            !t.IsAbstract &&      // 不能是抽象类
            //            !t.IsGenericType &&   // 不能是泛型类
            //            typeof(ILocalizationProvider).IsAssignableFrom(t) && // 实现接口
            //            t.GetCustomAttribute<LocalizationProviderAttribute>() != null // 带优先级属性
            //        );

            //        // 收集符合条件的类 + 优先级
            //        foreach (var type in types)
            //        {
            //            var attr = type.GetCustomAttribute<LocalizationProviderAttribute>();
            //            validProviders.Add((type, attr.Priority));
            //        }
            //    }
            //    catch
            //    {
            //        // 跳过加载异常的程序集
            //    }
            //}

            // 按优先级降序排序，返回最高优先级的类型
            return validProviders
                .OrderByDescending(x => x.priority)
                .FirstOrDefault()
                .type;
        }


        public static void SetLanguage(SystemLanguage language)
        {
            LanguageSettings settings = languages.FirstOrDefault(p => p.Language == language);
            if (settings == null)
            {
                Debug.LogError($"{language.ToString()}语言不存在！已切换到默认语言！");
                settings = defaultLanguage;
            }
                
            CurrentLanguage = settings;
            Initialized = true;
            PlayerPrefs.SetInt("language", (int)settings.Language);
            OnSetLanguage?.Invoke(settings.Language);
        }

        public static string GetDisplayName(SystemLanguage language)
        {
            LanguageSettings entry  = languages.Find(e => e != null && e.Language == language);
            return entry != null ? entry.GetDisplayName() : language.ToString() + "?";
        }

   


        #region 动态字典覆盖功能

        public static Dictionary<string, string> overrideTexts = new Dictionary<string, string>();

        public static void SetOverrideText(string key, string value)
        {
            overrideTexts[key] = value;
        }

        public static bool TryGetOverrideText(string key, out string value)
        {
            return overrideTexts.TryGetValue(key, out value);
        }

        public static bool RemoveOverrideText(string key)
        {
            return overrideTexts.Remove(key);
        }


        #endregion



        public static string GetPlainText(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "";
            key = key.Trim();

            if (TryGetOverrideText(key, out string overrideValue))
                return overrideValue;

            if (!Initialized)
                Initialize();

            string text = CurrentLanguage?.GetPlainText(key);
            if (text == null)
                text = defaultLanguage.GetPlainText(key);
            if (text == null)
                text = "*" + key + "*";
            return text;
        }


        public static string ToPlainText(this string key) => GetPlainText(key);
    }
}