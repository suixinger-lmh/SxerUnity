# Sxer.Plugin.EventSystem

#### 介绍
一个基于强类型设计的高性能 Unity 事件分发系统与面向对象的可视化指令（Command）配置框架。
* **强类型安全机制**：基于 CRTP 设计，无需手动声明和维护事件常数、ID 或 Key。
* **零 GC 延迟分发**：支持后台线程安全的分发机制，内部已配置对象池。在高频事件派发场景下，GC Alloc 为 **零**。
* **高效指令路由**：重构后支持 $O(1)$ 的高速指令匹配。
* **深度定制编辑器支持**：
  * 支持在 Inspector 上拖拽排序指令（基于 `ReorderableList`）。
  * 具备一键绑定特定分类下所有具体指令的快捷键。
  * 内置指令总览窗口（Command Center），可在场景中快速按需生成配置好的指令管理器。

#### 软件架构
EventManager：事件管理接口  提供事件注册，事件分发方法。  #AI提供的延迟派发逻辑代码，减少性能消耗
CoreEventDispatcher：实际的事件记录和管理对象。manager通过它进行事件注册和广播。

CoreEvent<T>:泛型事件，可动态扩展。 实现时可以添加实际事件所需参数。  manager根据id（type.fullname）归类相同的事件

CMD=》参考实际应用时，一条指令id对应一个处理。如：和web端js交互，两者通过具体指令，js端发送指令id，根据不同的指令id，创建不同的CMD来实现对应功能。
具体结构：
CMDEvent：CoreEvent<CMDEvent> 实现CoreEvent。  包含cmdid，和




##### 1. 事件系统 (`EventSystem`)
* **`CoreEvent<T>`**：自定义事件基类。
* **`CoreEventDispatcher`**：底层事件分发容器，支持线程锁。
* **`EventManager`**：主线程生命周期管理器，处理同步派发与下一帧延迟异步派发。

##### 2. 指令系统 (`CmdSystem`)
* **`CommandHandler` / `CommandHandlerBase<T>`**：业务指令的基类。
* **`CMDManager`**：挂载在物体上用于序列化和分发特定分类指令的组件。
* **`CommandDispatcher`**：管理并以 $O(1)$ 速度分发接收到的具体参数和指令。






#### 使用说明

##### 一、 事件系统使用
1声明一个事件
public class ScoreChangedEvent : CoreEvent<ScoreChangedEvent>
{
    public int CurrentScore;
    public ScoreChangedEvent(int score)
    {
        CurrentScore = score;
    }
}
2注册和注销事件
private void OnEnable()
{
    EventManager.Instance.AddEventListener<ScoreChangedEvent>(OnScoreChanged);
}

private void OnDisable()
{
    if (EventManager.Instance != null)
    {
        EventManager.Instance.RemoveEventListener<ScoreChangedEvent>(OnScoreChanged);
    }
}

private void OnScoreChanged(ScoreChangedEvent evt)
{
    Debug.Log($"当前分数更新为: {evt.CurrentScore}");
}
3分发事件
// 1. 同步派发（立即调用，非线程安全）
EventManager.Instance.DispatchCoreEventImmediately(new ScoreChangedEvent(100));

// 2. 延迟派发（推荐，下一帧 Update 运行，子线程安全，内部池化零 GC 消耗）
EventManager.Instance.DispatchCoreEvent(new ScoreChangedEvent(150));


##### 二、 指令系统使用
1=====声明自定义参数和指令分类
// 声明特定的自定义参数
public class MyCustomArg : CommandHandlerArg
{
    public float Speed;
}

// 声明这一大类指令对应的处理器基类
public abstract class MyCustomHandler : CommandHandlerBase<MyCustomArg> { }

2=====派生出具体的可执行子指令
public class CMD_SetSpeed : MyCustomHandler
{
    public override string CommandId => "set_speed"; // 你的指令唯一ID
    public override string CmdDesc => "设置移动速度";

    protected override void ExecuteAction(MyCustomArg arg)
    {
        Debug.Log($"[CMD] 执行修改移动速度，数值为: {arg.Speed}");
    }
}
3=====场景配置与触发
在场景中的任何物体上挂载 CMDManager。
在 Inspector 上选择你刚才声明的分类（例如：MyCustomHandler）。
使用编辑器的 一键绑定该类型下的所有指令，或通过 ReorderableList 单个添加你想要的具体指令。
游戏运行时，向事件系统抛出对应事件即可触发：
EventManager.Instance.DispatchCoreEvent(new CMDEvent("set_speed", new MyCustomArg { Speed = 5.5f }));

#### 记录




