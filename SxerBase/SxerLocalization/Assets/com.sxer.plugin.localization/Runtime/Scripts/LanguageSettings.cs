using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sxer.Plugin.Localization
{
    [Serializable]
    public class LanguageSettings
    {
        [SerializeField]
        private SystemLanguage language = SystemLanguage.Unknown;

        public SystemLanguage Language => language;

        public string DisplayName;

        private Dictionary<string, string> dic = new Dictionary<string, string>();


        public LanguageSettings(SystemLanguage language,string displayName)
        {
            this.language = language;
            this.DisplayName = displayName;
        }


        public string GetDisplayName() => this.DisplayName;
        public string GetPlainText(string key)
        {
            key = key.Trim();
            return Get(key);
        }


        public void Add(string key, string value) {

            if (string.IsNullOrEmpty(key))
                return;

            dic[key] = value;
        }

        private string Get(string key)
        {
            dic.TryGetValue(key, out string value);
            return value;
        }

        //private static string ConvertFromEscapes(string origin)
        //{
        //    if (string.IsNullOrEmpty(origin))
        //        return origin;
        //    return Regex.Unescape(origin);
        //}

        //private static string ConvertToEscapes(string origin)
        //{
        //    if (string.IsNullOrEmpty(origin))
        //        return origin;
        //    return Regex.Escape(origin);
        //}



        public bool HasKey(string key)
        {
            return dic.ContainsKey(key);
        }

        //public string GetPlainText(string key)
        //{
        //    //key = key.Trim();
        //    //return Provider.Get(key);
        //}

        //public string GetDisplayName() => GetPlainText("language_name");

        


    }
}