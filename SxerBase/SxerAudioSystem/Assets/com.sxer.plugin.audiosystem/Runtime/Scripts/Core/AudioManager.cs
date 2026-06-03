using UnityEngine;

namespace Sxer.Plugin.AudioSystem.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static AudioSystemBase _audioSystem;


        // 外部注入底层实现
        public static void Initialize(AudioSystemBase backend)
        {
            _audioSystem?.Release(); // 如果之前有旧的，先释放
            _audioSystem = backend;
            _audioSystem?.Initialize();
        }


        public static AudioManager Instance
        {
            get
            {
                if (_instance == null) Debug.LogError("AudioManager is not initialized!");
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            
            _audioSystem?.Update();
        }

        private void OnDestroy()
        {
            _audioSystem?.StopAll();
        }

        #region 对外提供的静态方法 (Facade)




        // 播放3D位置音效
        public static void Play(string eventPath, Vector3 position)
        {
            _audioSystem.PlayOneShot(eventPath, position);
        }

        // 播放2D/UI音效
        public static void Play(string eventPath)
        {
            _audioSystem.PlayOneShot(eventPath, Vector3.zero);
        }

        // 为物体挂载并获取独立的播放器
        public static IAudioPlayer GetOrCreatePlayer(GameObject target)
        {
            return _audioSystem.CreatePlayer(target);
        }

        // 总线控制
        public static IAudioBus GetBus(string busName)
        {
            return _audioSystem.GetBus(busName);
        }

        public static void SetGlobalParameter(string name, float value)
        {
            _audioSystem.SetGlobalParameter(name, value);
        }

        public static void StopAll() => _audioSystem?.StopAll();

        #endregion
    }
}