
namespace Sxer.Plugin.EventSystem
{
    //通过泛型类获取唯一id，达到动态扩展的功能
    public abstract class CoreEvent<T> where T : CoreEvent<T>
    {
        public static readonly string EventID = typeof(T).FullName;
    }

    
}