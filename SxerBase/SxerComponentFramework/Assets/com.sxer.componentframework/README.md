# sxercomponentframework

#### 介绍
Sxer.Frame 是一个轻量级、高度可扩展的 Unity 组件生命周期管理框架。它接管了 Unity 原生的 Awake/Start/Update，通过严格的状态机和优先级机制，解决大型游戏中组件初始化顺序混乱、异步依赖、以及内存泄漏的问题。

#### 软件架构
为了满足不同业务需求，框架将受管组件分为两类：
特性	单例组件 (Singleton Component)	动态组件 (Dynamic Component)
继承基类	SxerSingletonComponent<T>	SxerDynamicComponent
实例数量	全局唯一（1个）	全局可有多个（N个）
生命周期	随框架启动而初始化，随框架销毁而释放	运行时动态挂载、初始化、手动卸载销毁
挂载方式	通常挂载在框架预制体上自动搜集	动态 AddComponent 或从预制体实例化
获取方式	SxerFrame.Instance.GetSingleton<T>() 或 T.Instance	SxerFrame.Instance.GetDynamics<T>()
适用场景	音频管理、网络管理、UI系统、资源管理	玩家控制器、怪物AI、临时Buff管理器


#### 框架运行流程
启动阶段 (Initializing)：搜集场景中标记特性的单例组件 -> 按优先级排序 -> 依次执行 OnInit/OnInitAsync。
运行阶段 (Running)：每帧调用所有已初始化成功组件的 OnUpdate。支持运行时动态添加/卸载动态组件。
销毁阶段 (DisposeIng)：逆序（按优先级从大到小）执行所有组件的 OnDispose/OnDisposeAsync。

#### 使用说明
继承单例组件，需要挂在框架的子物体下，否则不会搜集

#### 结构
SxerComponentBase 组件基类
|-SxerSingletonComponent<T>  单例类型组件
|-SxerDynamicComponent  动态类型组件

SxerComponentAttribute 组件属性（优先级，组件描述，初始化类型）【用于编辑器扩展功能】

SxerFrame
|--StartupFramework() 初始化
|
|--GetSingletonComponent()
|
|--GetDynamicComponents()
|--AddDynamicComponent
|--RemoveDynamicComponent
|--ClearAllDynamicComponents()
|
|--ReloadSingletonComponent
|--ReloadDynamicComponent


