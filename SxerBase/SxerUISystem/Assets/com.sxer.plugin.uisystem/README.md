# SxerUISystem

#### 介绍
Sxer.UISystem 简单的UI管理系统

#### TD
边做边更新


	"dependencies": {
		"com.cysharp.unitask": "git@github.com:Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
	}
	
	
	
#### 软件架构
面向接口设计，将UI的关键功能以接口的形式实现。

1.动画接口
IUIAnimation：UI动画接口
IUITransitionAnimation：提供打开和关闭 两种UI动画，并接受动画执行完成后回调
IUILoopAnimation：循环动画的打开和关闭
IUIFeedbackAnimation：一次性触发的动画


2.UI业务接口（根据不同的业务逻辑，拆分接口）




#### 实际业务分析
1.游戏主界面的MainUI：



IUIParameter<T>（参数控制组件）：如音量滑动条、全屏开关、分辨率下拉框。
ITabPage（标签内容页）：如背包页、地图页、技能页。
ITabGroup（标签页组控制器）：负责管理并切换多个标签。
IUIView（独立视口）：如主菜单、HUD、设置弹窗。



#### 框架运行流程


#### 使用说明


#### 结构
UIElement UI元素基类   父节点，ui动画
|-UIPanel UI面板抽象类	CanvasGroup，退出按钮

UIAnimation UI动画抽象类  提供进入和退出两种动画

UIManager

#### 扩展
在实际的独立游戏开发中，UI 动画确实远比简单的“进入”和“退出”复杂。UI 动画按生命周期和触发时机，通常可以分为三类：
过渡动画（Transitions）：与 UI 的生命周期强绑定（进入、退出、暂停、恢复）。
状态动画（State/Idle Loops）：UI 处于某种状态时持续播放的动画（如：待机呼吸、高亮闪烁、循环旋转）。
瞬时反馈动画（Feedback/Triggers）：由玩家交互或事件临时触发的一次性动画（如：点击抖动、金币飞入、受到伤害闪红）。

✨ 核心特性
⚡ 彻底异步 (UniTask)：告别繁琐的 Callback 回调地狱，UI 的加载、打开、过渡动画、关闭全流程皆可 await，时序控制如丝般顺滑。
🧩 模块化与接口驱动：资源加载 (IUIResourceLoader)、UI动画 (IUIAnimation) 完全接口化，你可以无痛在 Resources/Addressables 之间切换，或自由更换动画插件 (DOTween/Animator)。
🛡️ 类型安全传参：拒绝 object[] 装箱拆箱，提供泛型基类 UIPanel<TData>，实现严格的强类型数据传递。
🎯 动画管线解耦：将 UI 动画严谨拆分为：过渡(Transition)、循环(Loop)、反馈(Feedback)，动画逻辑作为独立 Component 挂载，不污染 UI 业务代码。
📦 业务组件化：内置标准业务接口（如 ITabGroup, IUIParameter<T>），规范化复杂 UI 的内部通讯。
📂 目录结构
code
Text
Assets/
├── Scripts/
│   └── UI/
│       ├── Core/                     // 【核心层】(框架引擎)
│       │   ├── UIManager.cs          // UI主调度器 (管理面板生命周期与层级)
│       │   ├── UIElement.cs          // UI元素基类 (处理节点获取)
│       │   ├── UIPanel.cs            // UI面板基类 (处理CanvasGroup屏蔽与无参生命周期)
│       │   └── UIPanel_T.cs          // UI强类型面板基类 (继承UIPanel，处理带参数生命周期)
│       ├── Interfaces/               // 【抽象层】(能力接口)
│       │   ├── IUIResourceLoader.cs  // 资源加载器接口
│       │   ├── IUIAnimation.cs       // 动画接口组 (Transition/Loop/Feedback)
│       │   └── IUIComponents.cs      // UI业务能力接口 (ITabGroup, IUIParameter等)
│       ├── Implementations/          // 【实现层】(底层基建具体实现)
│       │   ├── Loaders/              // (如: AddressablesLoader, ResourcesLoader)
│       │   └── Animations/           // (如: DOTweenTransition, AnimatorTransition)
│       ├── Components/               // 【通用组件库】(可复用的轮子)
│       │   ├── UIParameters/         // 基础参数控件 (如: VolumeSlider实现IUIParameter)
│       │   └── TabSystem/            // 标签页系统
│       └── Views/                    // 【具体业务UI】(日常开发区)
│           └── SettingsUI/           // 示例：设置界面