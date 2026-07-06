using UnityEngine;
using Cysharp.Threading.Tasks;
using Sxer.Plugin.UISystem.Interfaces;

namespace Sxer.Plugin.UISystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeTransition : MonoBehaviour, IUITransitionAnimation
    {
        [Header("动画配置")]
        public float enterDuration = 0.3f;
        public float exitDuration = 0.2f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public async UniTask PlayEnterAsync()
        {
            canvasGroup.alpha = 0f;
            float time = 0f;

            // 原生 UniTask 异步差值，不依赖任何第三方插件
            while (time < enterDuration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / enterDuration);
                await UniTask.Yield(); // 等待下一帧
            }
            canvasGroup.alpha = 1f;
        }

        public async UniTask PlayExitAsync()
        {
            canvasGroup.alpha = 1f;
            float time = 0f;

            while (time < exitDuration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / exitDuration);
                await UniTask.Yield();
            }
            canvasGroup.alpha = 0f;
        }
    }
}