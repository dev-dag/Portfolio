using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionView : View
{
    [SerializeField] private Slider volumeSliderBGM;
    [SerializeField] private Slider volumeSliderSFX;
    [SerializeField] private Slider volumeSliderUI_SFX;

    public override void Init()
    {
        var cancelAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI").FindAction("Cancel");
        cancelAction.performed -= OnCancleInput;
        cancelAction.performed += OnCancleInput;

        volumeSliderBGM.onValueChanged.RemoveListener(OnBGM_SliderValueChanged);
        volumeSliderBGM.onValueChanged.AddListener(OnBGM_SliderValueChanged);

        volumeSliderSFX.onValueChanged.RemoveListener(OnSFX_SliderValueChanged);
        volumeSliderSFX.onValueChanged.AddListener(OnSFX_SliderValueChanged);

        volumeSliderUI_SFX.onValueChanged.RemoveListener(OnUI_SFX_SliderValueChanged);
        volumeSliderUI_SFX.onValueChanged.AddListener(OnUI_SFX_SliderValueChanged);

        base.Init();
    }

    public override void Show()
    {
        if (IsInit == false)
        {
            return;
        }

        base.Show();

        var instanceData = GameManager.Instance.InstanceData;
        volumeSliderBGM.SetValueWithoutNotify(instanceData.BGM_Volume);
        volumeSliderSFX.SetValueWithoutNotify(instanceData.SFX_Volume);
        volumeSliderUI_SFX.SetValueWithoutNotify(instanceData.UI_SFX_Volume);
    }

    /// <summary>
    /// OptionView를 여는 단축키가 입력되었을 때 호출되는 함수
    /// </summary>
    /// <param name="arg"></param>
    private void OnCancleInput(InputAction.CallbackContext arg)
    {
        if (this.gameObject.activeInHierarchy)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    /// <summary>
    /// BGM 슬라이더 값 변경 시 호출되는 함수
    /// </summary>
    private void OnBGM_SliderValueChanged(float value)
    {
        GameManager.Instance.InstanceData.SetBGM_Volume(value);
    }

    /// <summary>
    /// SFX 슬라이더 값 변경 시 호출되는 함수
    /// </summary>
    private void OnSFX_SliderValueChanged(float value)
    {
        GameManager.Instance.InstanceData.SetSFX_Volume(value);
    }

    /// <summary>
    /// UI SFX 슬라이더 값 변경 시 호출되는 함수
    /// </summary>
    private void OnUI_SFX_SliderValueChanged(float value)
    {
        GameManager.Instance.InstanceData.SetUI_SFX_Volume(value);
    }
}
