# SxerProcessManagement

#### 介绍
Sxer.ProcessManagement 流程控制

#### 软件架构
ProcessManager：流程总管理，负责搜集场景流程对象，控制流程的执行和取消。【同一时间只能由一个流程对象运行】
ProcessBase：提供流程的开始，暂停，取消等。  初始化在manager场景搜集完成时执行

目前实现两个流程变体：
FlowProcess：顺序流程=》子物体下所有实现了FlowStepBase的对象按照绑定顺序执行。

XMLProcess：LYT的流程框架，通过xml配置表，编辑器可视化配置。 实现process，operation，action的执行逻辑结构



#### TODO
添加单个流程完成回调
流程控制不完善
编辑器工具开发

#### 使用说明




