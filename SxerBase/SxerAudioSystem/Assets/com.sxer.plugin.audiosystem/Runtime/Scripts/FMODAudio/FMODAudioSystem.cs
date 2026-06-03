using FMOD.Studio;
using FMODUnity;
using Sxer.Plugin.AudioSystem.Core;
using UnityEngine;

namespace Sxer.Plugin.AudioSystem.FMODAudio
{
    // 实现总线逻辑
    public class FMODBus : IAudioBus
    {
        private Bus _bus;
        public string Name { get; }
        public FMODBus(string path)
        {
            Name = path;
            _bus = RuntimeManager.GetBus("bus:/" + path);
        }
        public float Volume { get => _bus.getVolume(out float v) == FMOD.RESULT.OK ? v : 1f; set => _bus.setVolume(value); }
        public bool IsMuted { get => _bus.getMute(out bool m) == FMOD.RESULT.OK ? m : false; set => _bus.setMute(value); }
    }


    public class FMODAudioSystem : AudioSystemBase
    {


        public override void Initialize()
        {
            // // FMOD Studio API 初始化通常由 RuntimeManager 自动处理
            //确保场景有RuntimeManager
        }
        public override void Release()
        {
            //StopAll();
        }


        public override IAudioPlayer CreatePlayer(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public override IAudioBus GetBus(string busName)
        {
            throw new System.NotImplementedException();
        }

       

        public override void PlayOneShot(string eventPath, Vector3 position = default)
        {
            RuntimeManager.PlayOneShot(eventPath, position);
        }

      

        public override void SetGlobalParameter(string name, float value)
        {
            throw new System.NotImplementedException();
        }

        public override void StopAll()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            
        }


    }

}
