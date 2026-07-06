using Cysharp.Threading.Tasks;
using Sxer.Plugin.UISystem.Core;
using Sxer.Plugin.UISystem.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Sxer.Plugin.UISystem
{
    // 定义传给UI的数据结构
    public struct SettingsData
    {
        public float bgmVolume;
        public float sfxVolume;
    }

    // 继承泛型基类，强制要求传入 SettingsData
    public class SettingsPanel : UIPanel<SettingsData>
    {
        [Header("业务组件引用")]
        public TabGroup tabGroup;        // 可以通过 Inspector 拖拽，也可以代码获取
        public Button btnApply;          // 应用按钮

        private IUIParameter<float> bgmSlider;
        private IUIParameter<float> sfxSlider;

        private SettingsData currentData;

        public override void OnInit()
        {
            // 在初始化时获取接口（解耦：我们不关心它是不是 Slider，只要实现了 IUIParameter<float> 即可）
            // 实际项目中，你可以用 transform.Find 获取，或者直接声明为 public 变量在面板拖拽
            bgmSlider = transform.Find("Content/AudioPage/BGMSlider")?.GetComponent<IUIParameter<float>>();
            sfxSlider = transform.Find("Content/AudioPage/SFXSlider")?.GetComponent<IUIParameter<float>>();

            btnApply.onClick.AddListener(OnApplyClicked);
        }

        // 强类型传参！没有装箱拆箱！
        public override void OnOpen(SettingsData data)
        {
            this.currentData = data;

            // 1. 刷新UI数据
            if (bgmSlider != null) bgmSlider.Value = data.bgmVolume;
            if (sfxSlider != null) sfxSlider.Value = data.sfxVolume;

            // 2. 重置标签页到第一页
            tabGroup?.SwitchTab(0);
        }

        private void OnApplyClicked()
        {
            // 从 UI 控件中读取最新数据
            currentData.bgmVolume = bgmSlider?.Value ?? 1f;
            currentData.sfxVolume = sfxSlider?.Value ?? 1f;

            Debug.Log($"保存设置 -> BGM: {currentData.bgmVolume}, SFX: {currentData.sfxVolume}");

            // 可以调用全局事件总线保存数据
            // EventBus.Emit(EventId.SaveSettings, currentData);

            // 保存后关闭自己
            UIManager.Instance.ClosePanelAsync<SettingsPanel>().Forget();
        }
    }
}