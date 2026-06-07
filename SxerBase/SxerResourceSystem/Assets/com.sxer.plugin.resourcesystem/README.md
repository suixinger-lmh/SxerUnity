# Sxer.Plugin.ResourceSystem
## 核心特性

1. **统一 URI 路由架构**  
   通过自定义前缀识别资源存储位置，自动分发到对应的 `ResourceProvider`（例如 `res://` 走向内置 Resources，`addr://` 走向 Addressables）。
   
2. **防重复加载与回调合并（Deduplication）**  
   对于相同资源的多次并发异步加载，系统只会触发一次底层 I/O，其余请求会自动排队并共享同一个回调。

3. **引用计数生命周期管理**  
   支持资源的精准引用计数控制（`Retain` 与 `Release`），并内置了针对安全周期的优化（避免加载中释放引起的对象池污染）。

4. **无 GC 句柄对象池**  
   句柄 `ResourceHandle` 采用 `UnityEngine.Pool.ObjectPool` 机制进行高频复用，将运行期运行时产生的 GC 垃圾降到最低。

5. **无主线程死锁设计**  
   优化了本地文件（File）同步加载逻辑，避免使用 `UnityWebRequest` 引起的自旋锁死锁现象。

---

## 路由协议说明

| 路由前缀 | 关联加载器 | 适用资源类型 | 说明 |
| :--- | :--- | :--- | :--- |
| `res://` | `UnityResourceProvider` | 任意内置资源 | 映射至 `Resources/` 目录 |
| `addr://` | `AddressablesProvider` | 任意寻址组资源 | 基于 Unity Addressables 寻址加载 |
| `file://` | `LocalFileProvider` | TextAsset / Texture2D / AudioClip | 加载设备本地绝对路径文件 |
| `stream://` | `StreamingAssetsProvider` | 多媒体/文本 | 加载 `StreamingAssets/` 目录内资源 |
| `http://` / `https://` | `WebProvider` | 纹理、音频、AssetBundle | 从远程服务器动态下载资源 |


#### 介绍
这是一个为 Unity 设计的轻量级、高度可扩展且健壮的资源管理系统。它基于 URI 路由和引用计数机制，能够屏蔽底层（Resources、Addressables、本地 File I/O、HTTP 网络下载）的差异，提供统一的高层加载接口。

#### 软件架构

ResourceManager：自动搜集子物体下所有资源提供对象，并执行对象初始化操作。
提供string扩展方法，路由路径自动修改。
AddLoad（把加载指令加入管理器，由管理器api统一触发实际下载【可控下载时机】）
Load（直接加载，异步和非异步）直接获取资源。




ResourceHandle：资源handle基类，参数：资源地址，资源引用数，缓存标记，handle状态，所属加载器对象，资源，完成事件回调。方法：释放，销毁，完成调用接口，


ResourceProviderBase：加载器基类，路由前缀参数。提供加载方法，卸载方法。以及面向handle的加载（顺序加载或者同步加载）等方法。
由具体实现完成不同加载器（例如：untiy Addressable加载，Resource加载，StreamingAssets加载，web加载，本地文件加载，程序化加载）等方式。




#### 使用说明

每种加载器只需要一个实例即可，所有加载指令通过handle的方式，由用户调用manager接口时创建，在加载器里管理。

### 1. 同步加载
适用于需要立即获取并使用的资源（本地或 Resources 资源）：
// 加载 Resources 目录下的 Prefab
ResourceHandle handle = ResourceManager.Instance.Load<GameObject>("res://Prefabs/Player");

if (handle.State == ResourceState.Success)
{
    GameObject prefab = handle.Get<GameObject>();
    GameObject instance = Instantiate(prefab);
}
不使用时，请记得释放引用
handle.Release();

### 2. 异步加载
适用于大型资源或网络、本地文件的读取，避免卡顿：
ResourceManager.Instance.LoadAsync<Texture2D>("file://C:/Users/Admin/Pictures/photo.png", (texture) =>
{
    if (texture != null)
    {
        myRawImage.texture = texture;
    }
});

### 3. 延迟加载 (AddLoad + StartLoad)
支持分步初始化，您可以先生成句柄，并在需要时统一触发加载：
// 1. 生成句柄并注册回调，此时不触发实质 I/O
ResourceHandle handle = ResourceManager.Instance.AddLoad<AudioClip>("http://example.com/bgm.mp3");
handle.AddCallback(h =>
{
    if (h.State == ResourceState.Success)
    {
        myAudioSource.clip = h.Get<AudioClip>();
        myAudioSource.Play();
    }
});

// 2. 准备妥当后，在合适时机开始加载
ResourceManager.Instance.StartLoad(handle, async: true);

### 4. 资源卸载
// 方式 A：通过句柄显式释放
handle.Release();

// 方式 B：通过 URI 释放（需提供类型）
ResourceManager.Instance.Release("res://Prefabs/Player", typeof(GameObject));

// 方式 C：通过已实例化的 Asset 对象直接匹配并释放
ResourceManager.Instance.Release(myTexture2D);


#### 记录




