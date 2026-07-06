using Sxer.Plugin.UISystem.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Sxer.Plugin.UISystem 
{
    [RequireComponent(typeof(Slider))]
    public class SliderParameter : MonoBehaviour, IUIParameter<float>
    {
        private Slider slider;

        public float Value
        {
            get => slider.value;
            set => slider.value = value;
        }

        private void Awake()
        {
            slider = GetComponent<Slider>();
            // 监听UI变化并抛出
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        public void OnValueChanged(float newValue)
        {
            // 此处可以抛出全局事件，或者由外层业务读取
            // Debug.Log($"[SliderParameter] 数值变为: {newValue}");
        }
    }
}