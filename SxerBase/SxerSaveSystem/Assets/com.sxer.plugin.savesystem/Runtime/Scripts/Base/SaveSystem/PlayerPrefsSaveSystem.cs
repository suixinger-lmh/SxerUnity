using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Sxer.Plugin.SaveSystem
{
    public class PlayerPrefsSaveSystem : MonoBehaviour, ISaveSystem
    {
        public void Delete(string absolutePath, string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public bool Exists(string absolutePath, string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public T Load<T>(string absolutePath, string key) where T : class, new()
        {
            if (!Exists(absolutePath,key)) return new T();
            return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key)) ?? new T();
        }

        public Task<T> LoadAsync<T>(string absolutePath, string key) where T : class, new()
        {
            return Task.FromResult(Load<T>(absolutePath, key));
        }

        public void Save<T>(string absolutePath, string key, T data)
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public Task SaveAsync<T>(string absolutePath, string key, T data)
        {
            Save(absolutePath, key, data); // PlayerPrefs不支持真异步，直接同步执行后返回完成的任务
            return Task.CompletedTask;
        }
    }
}