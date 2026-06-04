# SxerLocalization

#### 介绍
Sxer.Localization 语言本地化

使用Unity的SystemLanguage作为语言分类

#### 软件架构
LocalizationManager：静态管理类。给string字符串提供ToPlainText方法。
初始化时会查找当前程序集，找到优先级最高的提供对象。生成该对象实例，通过该对象拿到所有语言集合
提供OnSetLanguage  修改语言回调
提供动态覆盖功能，向动态覆盖字典里添加，可屏蔽语言集合翻译

LanguageSettings：一种语言集合，包含当前语言信息，以及所有的对应文字字典。提供查找方法

ILocalizationProvider：实际本地化功能提供对象接口。提供GetAllLanguage方法，返回解析后的所有语言集合LanguageSettings




#### 扩展
BaseLocalizationProvider：实现了一个默认提供对象，Resource下Language文件，以json格式形成本地化数据
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
 
 json：
 {
    "Items": [
		{
            "Key": "Language",
			"Languages": [
				"ChineseSimplified",
				"English",
				"ChineseTraditional"
			]
        },
		{
            "Key": "Language_Display",
			"Languages": [
				"简体中文",
				"英文",
				"繁体中文"
			]
        },
        {
            "Key": "Btn_StartGame",
			"Languages": [
				"开始游戏",
				"Start Game",
				"開始遊戲"
			]
        },
        {
            "Key": "Btn_Setting",
			"Languages": [
				"设置",
				"Setting",
				"設定"
			]
        },
        {
            "Key": "Tip_ExitGame",
			"Languages": [
				"确定退出游戏吗？",
				"Are you sure to exit?",
				"確定退出遊戲嗎？"
			]
        }
    ]
}


#### TODO
添加单个流程完成回调
流程控制不完善
编辑器工具开发

#### 使用说明




