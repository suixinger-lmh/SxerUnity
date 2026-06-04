# SxerObjectPool

#### 介绍
Sxer.ObjectPool 对象池


#### 软件架构

Unity实现的UnityEngine.Pool感觉蛮好的

只需要多加一层封装来使用即可。不关系底部逻辑，只关心节点的事件执行即可

#### 扩展

这里参考《逃离鸭科夫》的封装
PrefabPool：针对unity组件的对象池封装
ClassPool：所有类型的对象池封装
原理一致

IPoolable：给池对象实现的接口，在池释放或添加时执行的具体操作


#### 使用说明

 public class TestClass :IPoolable{
     public string name;

     public void NotifyPooled()
     {
         name = "111";
     }

     public void NotifyReleased()
     {
         name = string.Empty;
     }
 }
 public ClassPool<TestClass> classPool = new ClassPool<TestClass>();

 public PrefabPool<AudioListener> acPool;



