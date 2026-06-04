
using System.Threading.Tasks;

namespace Sxer.Plugin.SaveSystem
{
    // µ×²ã´æ´¢½Ó¿Ú
    public interface ISaveSystem
    {
        void Save<T>(string absolutePath, string key, T data);
        Task SaveAsync<T>(string absolutePath, string key,T data);
        
        T Load<T>(string absolutePath, string key) where T : class, new();
        Task<T> LoadAsync<T>(string absolutePath, string key) where T : class, new();
        void Delete(string absolutePath, string key);
        bool Exists(string absolutePath, string key);

    }
}