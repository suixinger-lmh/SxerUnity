using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Sxer.Plugin.SaveSystem
{
    // 具体的JSON存储实现（带有防损坏机制）
    public class JsonSaveSystem : ISaveSystem
    {
        public void Save<T>(string absolutePath, string key, T data)
        {
            string json = JsonUtility.ToJson(data);
            SafeWrite(absolutePath, json);
        }

        public async Task SaveAsync<T>(string absolutePath, string key, T data)
        {
            // 将序列化放在后台线程，防止大文件卡主线程
            string json = await Task.Run(() => JsonUtility.ToJson(data));
            await SafeWriteAsync(absolutePath, json);
        }


        public T Load<T>(string absolutePath, string key) where T : class, new()
        {
            if (!Exists(absolutePath, key)) return new T(); // 文件不存在时返回默认对象
            string json = File.ReadAllText(absolutePath);
            return JsonUtility.FromJson<T>(json) ?? new T();
        }

     

        public async Task<T> LoadAsync<T>(string absolutePath, string key) where T : class, new()
        {
            if (!Exists(absolutePath,key)) return new T();
            using StreamReader reader = new StreamReader(absolutePath);
            string json = await reader.ReadToEndAsync();
            return await Task.Run(() => JsonUtility.FromJson<T>(json)) ?? new T();
        }
        public void Delete(string absolutePath, string key)
        {
            if (File.Exists(absolutePath)) File.Delete(absolutePath);
        }

        public bool Exists(string absolutePath, string key)
        {
            return File.Exists(absolutePath);
        }




        // 防损坏写入机制 (Safe IO)
        private void SafeWrite(string path, string content)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            File.WriteAllText(tempPath, content);
            // 如果原文件存在，先备份原文件，再将临时文件替换为正式文件
            if (File.Exists(path)) File.Replace(tempPath, path, backupPath);
            else File.Move(tempPath, path);
        }

        private async Task SafeWriteAsync(string path, string content)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            using (StreamWriter writer = new StreamWriter(tempPath))
            {
                await writer.WriteAsync(content);
            }

            if (File.Exists(path)) File.Replace(tempPath, path, backupPath);
            else File.Move(tempPath, path);
        }

    }
}