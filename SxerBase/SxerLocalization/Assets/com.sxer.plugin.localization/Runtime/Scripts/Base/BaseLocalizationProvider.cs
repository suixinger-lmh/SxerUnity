using System;
using System.Collections.Generic;
using UnityEngine;


namespace Sxer.Plugin.Localization.Json
{
    [Serializable]
    public class LocalizationItem
    {
        public string Key;
        public List<string> Languages;
    }

    [Serializable]
    public class LocalizationDatabase
    {
        public List<LocalizationItem> Items;
    }

    [LocalizationProvider(1)]
    public class BaseLocalizationProvider : ILocalizationProvider
    {
        public TextAsset text;

        public LocalizationDatabase localizationDatabase;
        //private string path = Path.Combine(Application.streamingAssetsPath, "Localization/Language.json");

       


        private void GetText() {

            text = Resources.Load<TextAsset>("Language");
            if(text == null)
                Debug.LogError("需要配置文件Language。放在Resources文件夹里。");

        }




        public List<LanguageSettings> GetAllLanguage()
        {
            GetText();

            localizationDatabase = JsonUtility.FromJson<LocalizationDatabase>(text.text);

            LocalizationItem languageItem = localizationDatabase.Items.Find(p => p.Key == "Language");
            LocalizationItem languageDisplay = localizationDatabase.Items.Find(p => p.Key == "Language_Display");
            if (languageItem == null)
            {
                Debug.LogError("语言信息不存在！本地化失败！");
                return null;
            }

            int languageCount = languageItem.Languages.Count;
            List<LanguageSettings> localizationDic = new List<LanguageSettings>();
            for (int i = 0; i < languageCount; i++) {
                try
                {
                    string LanguageName = languageItem.Languages[i];
                    string LanguageDisplay = languageDisplay.Languages[i];
                    if (!Enum.TryParse(LanguageName, out SystemLanguage systemLanguage)) 
                    {
                        Debug.LogError($"语言类型获取失败！{LanguageName}");
                        continue;
                    }

                    LanguageSettings languageSet = new LanguageSettings(systemLanguage, LanguageDisplay);

                    foreach (var item in localizationDatabase.Items)
                    {
                        try
                        {
                            languageSet.Add(item.Key, item.Languages[i]);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Debug.LogError($"{item.Key}对应的{i}不存在！");
                            continue;
                        }
                    }

                    localizationDic.Add(languageSet);
                }
                catch {
                    Debug.LogError($"序号{i}的语言获取失败！");
                    continue;
                }
            }
            return localizationDic;
        }
    }
}