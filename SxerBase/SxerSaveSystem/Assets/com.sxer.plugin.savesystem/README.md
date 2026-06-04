# SxerSaveSystem

#### 介绍
Sxer.SaveSystem 数据持久化

分为数据存储实际提供对象+存储管线（即一种存储逻辑，例如：存档，游戏系统环境配置的保存，其他的本地持久化存储逻辑）



#### 软件架构
SaveManager：初始化时搜集自身子物体下实现的具体存储系统。


SaveLineBase：存储管线，一个管线代表一个实际的存储逻辑，例如：在注册表保存和获取环境配置；在指定文件夹下存储存档；等
持有具体的存储对象

ISaveSystem：存储对象接口，继承该接口来提供具体的存储功能。
加载，保存，删除，查询


独立：
ISaveDataProvider<T>:数据转换接口   给具体的业务对象绑定，提供保存数据的提取和绑定


#### 扩展
先粗略实现了两个存储系统：
JsonSaveSystem：通过JsonUtility实现的json存储，同时提供防损坏写入机制  创建.tmp和.bak备份操场
PlayerPrefsSaveSystem：Unity的PlayerPrefs操作，即注册表存储信息

#### 

#### 使用说明

1.实现具体的存储系统功能
ISaveSystem
将实现好的ISaveSystem存储对象挂载manager子物体下，或者运行时动态添加进去。

2.针对业务逻辑创建不同的存储管线（同时可以绑定不同的数据结构）
SaveLineBase

3.通过SaveManager生成管线对象，并通过管线执行保存加载操作
SaveManager.CreateSaveLine<>();


例：
SaveManager.Instance.AddSaveSystem(new JsonSaveSystem());
SaveManager.Instance.AddSaveSystem(new PlayerPrefsSaveSystem());
   


public class TestSaveLine : SaveLine<TestDddData>
{
    [Serializable]
    public class TestDddData {
        public string name = "";
        public int index = 0;
    }
}

//创建存储管线，绑定实际存储系统
TestSaveLine line2 = SaveManager.Instance.CreateSaveLine<TestSaveLine, JsonSaveSystem>("测试2", "C:\\Users\\Sxer\\Desktop\\tt.json", "");
TestDddData xx = new TestDddData();
xx.name = "li";
line2.ChangeCacheData(xx);
line2.Save();


 



