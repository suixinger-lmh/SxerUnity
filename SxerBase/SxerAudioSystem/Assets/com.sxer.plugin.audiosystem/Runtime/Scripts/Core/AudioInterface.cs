using UnityEngine;

namespace Sxer.Plugin.AudioSystem.Core
{
    // 音频总线接口 (对应 Master, SFX, Music)
    public interface IAudioBus
    {
        string Name { get; }
        float Volume { get; set; } // 范围 0.0 ~ 1.0
        bool IsMuted { get; set; }
    }

    // 音频实体接口 (绑定在具体的GameObject上，控制长音频或循环音频)
    public interface IAudioPlayer
    {
        void Play(string eventPath, bool loop = false);
        void Stop(bool immediate = false);
        void SetParameter(string name, float value);
        bool IsPlaying { get; }
    }

    // 【核心】抽象基类，所有底层引擎都要继承它
    public abstract class AudioSystemBase
    {
        public abstract void Initialize();
        public abstract void Update();
        public abstract void Release();

        public abstract IAudioBus GetBus(string busName);
        public abstract IAudioPlayer CreatePlayer(GameObject target);
        public abstract void PlayOneShot(string eventPath, Vector3 position = default);
        public abstract void SetGlobalParameter(string name, float value);
        public abstract void StopAll();

        // 相比于接口，基类的优势：可以放一些公共工具方法供子类复用！
        protected float LinearToDb(float linear) => linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
        protected float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);
    }
}